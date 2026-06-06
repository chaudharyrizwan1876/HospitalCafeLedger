using HospitalCafeLedger.Data;

namespace HospitalCafeLedger.Services;

public class DashboardStats
{
    public decimal TodaySales         { get; set; }
    public decimal TotalPending       { get; set; }  // net amount owed across all doctors
    public int     TotalDoctors       { get; set; }
    public int     TodayOrders        { get; set; }
    public decimal TotalAdvanceCredit { get; set; }  // doctors with positive balance
    public int     DoctorsWithPending { get; set; }
}

public class RecentTransactionVM
{
    public string DoctorName  { get; set; } = "";
    public string ItemsSummary{ get; set; } = "";
    public decimal Amount     { get; set; }
    public DateTime OrderDate { get; set; }
    public string AmountDisplay  => $"Rs. {Amount:N0}";
    public string DateDisplay    => OrderDate.ToString("dd MMM yyyy  hh:mm tt");
}

public class TopItemVM
{
    public string Name       { get; set; } = "";
    public int    OrderCount { get; set; }
    public double BarWidth   { get; set; }  // 0-1 ratio for bar fill
}

public class DailyAmountVM
{
    public int     Day    { get; set; }
    public decimal Amount { get; set; }
}

public class DashboardService
{
    public DashboardStats GetStats()
    {
        using var db = new AppDbContext();
        var today = DateTime.Today;

        // Today sales = sum of all order amounts placed today
        var todaySales = db.Orders
            .Where(o => o.OrderDate.Date == today)
            .Sum(o => (decimal?)(o.Quantity * o.Price)) ?? 0m;

        // Today orders count
        var todayOrders = db.Orders
            .Count(o => o.OrderDate.Date == today);

        // Total doctors (active)
        var totalDoctors = db.Doctors.Count(d => d.IsActive);

        // Per doctor: wallet balance = opening + cash payments - all orders
        var doctors  = db.Doctors.Where(d => d.IsActive).ToList();
        var payments = db.Payments.ToList();
        var orders   = db.Orders.ToList();

        decimal totalPending = 0;
        decimal totalCredit  = 0;
        int     pendingCount = 0;

        foreach (var doc in doctors)
        {
            var deposited = doc.OpeningBalance
                + payments.Where(p => p.DoctorId == doc.Id).Sum(p => p.Amount);
            var spent = orders.Where(o => o.DoctorId == doc.Id)
                              .Sum(o => o.Quantity * o.Price);
            var balance = deposited - spent;

            if (balance < 0)
            {
                totalPending += Math.Abs(balance);
                pendingCount++;
            }
            else
            {
                totalCredit += balance;
            }
        }

        return new DashboardStats
        {
            TodaySales         = todaySales,
            TotalPending       = totalPending,
            TotalDoctors       = totalDoctors,
            TodayOrders        = todayOrders,
            TotalAdvanceCredit = totalCredit,
            DoctorsWithPending = pendingCount
        };
    }

    // Daily order amounts for current month (for chart)
    public List<DailyAmountVM> GetMonthlyDailyAmounts(int month, int year)
    {
        using var db = new AppDbContext();
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var orders = db.Orders
            .Where(o => o.OrderDate.Month == month && o.OrderDate.Year == year)
            .ToList();

        return Enumerable.Range(1, daysInMonth).Select(day => new DailyAmountVM
        {
            Day    = day,
            Amount = orders
                .Where(o => o.OrderDate.Day == day)
                .Sum(o => o.Quantity * o.Price)
        }).ToList();
    }

    // Top 5 items by order quantity this month
    public List<TopItemVM> GetTopItems(int month, int year)
    {
        using var db = new AppDbContext();

        var orders = db.Orders
            .Where(o => o.OrderDate.Month == month && o.OrderDate.Year == year)
            .ToList();

        var itemIds = orders.Select(o => o.ItemId).Distinct().ToList();
        var items   = db.Items.Where(i => itemIds.Contains(i.Id))
                              .ToDictionary(i => i.Id, i => i.Name);

        var grouped = orders
            .GroupBy(o => o.ItemId)
            .Select(g => new
            {
                Name  = items.TryGetValue(g.Key, out var n) ? n : "Custom Item",
                Count = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        if (grouped.Count == 0) return new List<TopItemVM>();

        var maxCount = grouped.Max(x => x.Count);
        return grouped.Select(g => new TopItemVM
        {
            Name       = g.Name,
            OrderCount = g.Count,
            BarWidth   = maxCount > 0 ? (double)g.Count / maxCount : 0
        }).ToList();
    }

    // Recent transactions (last 5 orders grouped by doctor+date)
    public List<RecentTransactionVM> GetRecentTransactions()
    {
        using var db = new AppDbContext();

        var doctors = db.Doctors.ToDictionary(d => d.Id, d => d.Name);
        var items   = db.Items.ToDictionary(i => i.Id, i => i.Name);

        // Last 10 orders
        var recent = db.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(20)
            .ToList();

        // Group by doctor + same minute (so one billing session = one row)
        return recent
            .GroupBy(o => new { o.DoctorId, Slot = new DateTime(
                o.OrderDate.Year, o.OrderDate.Month, o.OrderDate.Day,
                o.OrderDate.Hour, o.OrderDate.Minute, 0) })
            .Take(5)
            .Select(g =>
            {
                var itemSummary = string.Join(", ", g.Select(o =>
                {
                    var name = items.TryGetValue(o.ItemId, out var n) ? n : "Item";
                    return $"{name} x{o.Quantity}";
                }));
                return new RecentTransactionVM
                {
                    DoctorName   = doctors.TryGetValue(g.Key.DoctorId, out var dn) ? dn : "Unknown",
                    ItemsSummary = itemSummary,
                    Amount       = g.Sum(o => o.Quantity * o.Price),
                    OrderDate    = g.Key.Slot
                };
            })
            .OrderByDescending(t => t.OrderDate)
            .ToList();
    }
}
