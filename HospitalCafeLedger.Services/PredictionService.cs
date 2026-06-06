using HospitalCafeLedger.Data;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace HospitalCafeLedger.Services;

// ── Input/Output classes for ML.NET ──────────────────────────

public class SalesData
{
    public float Month  { get; set; }
    public float Amount { get; set; }
}

public class SalesPrediction
{
    [ColumnName("Score")]
    public float PredictedAmount { get; set; }
}

// ── Result ViewModels returned to UI ─────────────────────────

public class NextMonthSalesPrediction
{
    public string MonthName       { get; set; } = "";
    public decimal PredictedAmount { get; set; }
    public decimal LastMonthAmount { get; set; }
    public decimal ChangePercent   { get; set; }
    public bool    IsIncrease      { get; set; }
    public string  Confidence      { get; set; } = "";
}

public class DoctorBalanceWarning
{
    public string  DoctorName        { get; set; } = "";
    public decimal CurrentBalance    { get; set; }
    public decimal AvgDailySpend     { get; set; }
    public int     DaysUntilEmpty    { get; set; }
    public string  WarningLevel      { get; set; } = ""; // "Critical","Warning","OK"
}

public class ItemTrendPrediction
{
    public string  ItemName         { get; set; } = "";
    public int     CurrentMonthQty  { get; set; }
    public int     PredictedNextQty { get; set; }
    public string  Trend            { get; set; } = ""; // "Rising","Falling","Stable"
    public decimal TrendPercent     { get; set; }
}

public class PeakPeriodResult
{
    public string PeakDay   { get; set; } = "";   // e.g. "Monday"
    public string PeakHour  { get; set; } = "";   // e.g. "12:00 - 13:00"
    public int    PeakDayOrders  { get; set; }
    public int    PeakHourOrders { get; set; }
    public List<DayOrderCount>  DayCounts  { get; set; } = new();
    public List<HourOrderCount> HourCounts { get; set; } = new();
}

public class DayOrderCount
{
    public string DayName   { get; set; } = "";
    public int    Count     { get; set; }
    public double BarRatio  { get; set; }
}

public class HourOrderCount
{
    public string HourLabel { get; set; } = "";
    public int    Count     { get; set; }
    public double BarRatio  { get; set; }
}

// ── PredictionService ─────────────────────────────────────────

public class PredictionService
{
    private readonly MLContext _ml = new(seed: 42);

    // ── 1. Next Month Sales Prediction ───────────────────────
    public NextMonthSalesPrediction PredictNextMonthSales()
    {
        using var db = new AppDbContext();

        // Get monthly totals for last 12 months
        var now    = DateTime.Now;
        var months = Enumerable.Range(0, 12)
            .Select(i => now.AddMonths(-11 + i))
            .Select(d => new SalesData
            {
                Month  = d.Month + (d.Year - now.Year) * 12f,
                Amount = (float)(db.Orders
                    .Where(o => o.OrderDate.Month == d.Month && o.OrderDate.Year == d.Year)
                    .Sum(o => (decimal?)(o.Quantity * o.Price)) ?? 0m)
            }).ToList();

        decimal lastMonthAmount = (decimal)(months.LastOrDefault()?.Amount ?? 0f);
        decimal predictedAmount;
        string  confidence;

        // Need at least 3 months of data for ML model
        var nonZero = months.Count(m => m.Amount > 0);
        if (nonZero >= 3)
        {
            try
            {
                var data     = _ml.Data.LoadFromEnumerable(months);
                var pipeline = _ml.Transforms.Concatenate("Features", nameof(SalesData.Month))
                    .Append(_ml.Regression.Trainers.Sdca(
                        labelColumnName:   nameof(SalesData.Amount),
                        featureColumnName: "Features"));

                var model   = pipeline.Fit(data);
                var engine  = _ml.Model.CreatePredictionEngine<SalesData, SalesPrediction>(model);
                var nextMonth = now.AddMonths(1);
                var result  = engine.Predict(new SalesData
                {
                    Month = nextMonth.Month + (nextMonth.Year - now.Year) * 12f
                });
                predictedAmount = Math.Max(0, (decimal)result.PredictedAmount);
                confidence = nonZero >= 6 ? "High" : "Medium";
            }
            catch
            {
                // Fallback: simple average of last 3 months
                predictedAmount = (decimal)months.TakeLast(3).Average(m => m.Amount);
                confidence = "Low";
            }
        }
        else
        {
            // Not enough data — use available average
            predictedAmount = nonZero > 0
                ? (decimal)months.Where(m => m.Amount > 0).Average(m => m.Amount)
                : 0m;
            confidence = "Low (insufficient data)";
        }

        var change  = lastMonthAmount > 0
            ? ((predictedAmount - lastMonthAmount) / lastMonthAmount) * 100
            : 0m;

        var monthNames = new[]
        {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };

        return new NextMonthSalesPrediction
        {
            MonthName       = monthNames[now.AddMonths(1).Month - 1],
            PredictedAmount = Math.Round(predictedAmount),
            LastMonthAmount = lastMonthAmount,
            ChangePercent   = Math.Round(Math.Abs(change), 1),
            IsIncrease      = change >= 0,
            Confidence      = confidence
        };
    }

    // ── 2. Doctor Balance Warnings ────────────────────────────
    public List<DoctorBalanceWarning> GetDoctorBalanceWarnings()
    {
        using var db = new AppDbContext();

        var doctors  = db.Doctors.Where(d => d.IsActive).ToList();
        var payments = db.Payments.ToList();
        var orders   = db.Orders.ToList();
        var warnings = new List<DoctorBalanceWarning>();
        var cutoff   = DateTime.Now.AddDays(-30);

        foreach (var doc in doctors)
        {
            var deposited = doc.OpeningBalance
                + payments.Where(p => p.DoctorId == doc.Id).Sum(p => p.Amount);
            var totalSpent = orders
                .Where(o => o.DoctorId == doc.Id)
                .Sum(o => o.Quantity * o.Price);
            var balance = deposited - totalSpent;

            // Avg daily spend over last 30 days
            var recentSpend = orders
                .Where(o => o.DoctorId == doc.Id && o.OrderDate >= cutoff)
                .Sum(o => o.Quantity * o.Price);
            var avgDaily = recentSpend / 30m;

            int daysLeft = avgDaily > 0
                ? (int)Math.Floor(balance / avgDaily)
                : (balance > 0 ? 999 : 0);

            string level;
            if (balance <= 0)
                level = "Critical";
            else if (daysLeft <= 7)
                level = "Critical";
            else if (daysLeft <= 15)
                level = "Warning";
            else
                level = "OK";

            if (level != "OK")
            {
                warnings.Add(new DoctorBalanceWarning
                {
                    DoctorName     = doc.Name,
                    CurrentBalance = balance,
                    AvgDailySpend  = Math.Round(avgDaily, 0),
                    DaysUntilEmpty = daysLeft < 0 ? 0 : daysLeft,
                    WarningLevel   = level
                });
            }
        }

        return warnings
            .OrderBy(w => w.WarningLevel == "Critical" ? 0 : 1)
            .ThenBy(w => w.DaysUntilEmpty)
            .ToList();
    }

    // ── 3. Top Items Trend ────────────────────────────────────
    public List<ItemTrendPrediction> GetItemTrends()
    {
        using var db = new AppDbContext();

        var now       = DateTime.Now;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        var orders    = db.Orders.ToList();
        var items     = db.Items.ToDictionary(i => i.Id, i => i.Name);

        // Group by item — this month vs last month
        var allItemIds = orders.Select(o => o.ItemId).Distinct().ToList();
        var trends     = new List<ItemTrendPrediction>();

        foreach (var itemId in allItemIds)
        {
            var name = items.TryGetValue(itemId, out var n) ? n : "Custom Item";

            var thisQty = orders
                .Where(o => o.ItemId == itemId
                         && o.OrderDate >= thisMonth
                         && o.OrderDate < thisMonth.AddMonths(1))
                .Sum(o => o.Quantity);

            var lastQty = orders
                .Where(o => o.ItemId == itemId
                         && o.OrderDate >= lastMonth
                         && o.OrderDate < thisMonth)
                .Sum(o => o.Quantity);

            // Simple prediction: if rising, predict +20%; if falling -20%; stable ±5%
            decimal changePct = lastQty > 0
                ? ((decimal)(thisQty - lastQty) / lastQty) * 100
                : (thisQty > 0 ? 100m : 0m);

            string trend;
            int predictedQty;
            if (changePct > 10)
            {
                trend        = "Rising ↑";
                predictedQty = (int)Math.Round(thisQty * 1.2m);
            }
            else if (changePct < -10)
            {
                trend        = "Falling ↓";
                predictedQty = (int)Math.Round(thisQty * 0.8m);
            }
            else
            {
                trend        = "Stable →";
                predictedQty = thisQty;
            }

            if (thisQty > 0 || lastQty > 0)
            {
                trends.Add(new ItemTrendPrediction
                {
                    ItemName         = name,
                    CurrentMonthQty  = thisQty,
                    PredictedNextQty = predictedQty,
                    Trend            = trend,
                    TrendPercent     = Math.Round(Math.Abs(changePct), 1)
                });
            }
        }

        return trends.OrderByDescending(t => t.CurrentMonthQty).Take(8).ToList();
    }

    // ── 4. Peak Days & Hours ──────────────────────────────────
    public PeakPeriodResult GetPeakPeriods()
    {
        using var db = new AppDbContext();

        var orders = db.Orders
            .Where(o => o.OrderDate >= DateTime.Now.AddDays(-60))
            .ToList();

        if (orders.Count == 0)
            return new PeakPeriodResult
            {
                PeakDay = "No data", PeakHour = "No data"
            };

        // Day of week counts
        var dayGroups = orders
            .GroupBy(o => o.OrderDate.DayOfWeek)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var maxDay = dayGroups.Max(d => d.Count);
        var dayCounts = Enum.GetValues<DayOfWeek>()
            .Select(d =>
            {
                var c = dayGroups.FirstOrDefault(g => g.Day == d)?.Count ?? 0;
                return new DayOrderCount
                {
                    DayName  = d.ToString(),
                    Count    = c,
                    BarRatio = maxDay > 0 ? (double)c / maxDay : 0
                };
            }).ToList();

        // Hour counts (grouped into 1-hour slots)
        var hourGroups = orders
            .GroupBy(o => o.OrderDate.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToList();

        var maxHour = hourGroups.Count > 0 ? hourGroups.Max(h => h.Count) : 1;
        var hourCounts = Enumerable.Range(7, 14) // 7am to 9pm
            .Select(h =>
            {
                var c = hourGroups.FirstOrDefault(g => g.Hour == h)?.Count ?? 0;
                return new HourOrderCount
                {
                    HourLabel = $"{h:00}:00",
                    Count     = c,
                    BarRatio  = maxHour > 0 ? (double)c / maxHour : 0
                };
            }).ToList();

        var peakDay  = dayGroups.FirstOrDefault();
        var peakHour = hourGroups.OrderByDescending(h => h.Count).FirstOrDefault();

        return new PeakPeriodResult
        {
            PeakDay        = peakDay?.Day.ToString() ?? "—",
            PeakHour       = peakHour != null ? $"{peakHour.Hour:00}:00 - {peakHour.Hour+1:00}:00" : "—",
            PeakDayOrders  = peakDay?.Count ?? 0,
            PeakHourOrders = peakHour?.Count ?? 0,
            DayCounts      = dayCounts,
            HourCounts     = hourCounts
        };
    }
}
