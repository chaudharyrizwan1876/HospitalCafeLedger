using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

// ── UI ViewModels ─────────────────────────────────────────────

public class WarningRowVM
{
    public string DoctorName   { get; set; } = "";
    public string BalanceText  { get; set; } = "";
    public string DaysText     { get; set; } = "";
    public string WarningLevel { get; set; } = "";
}

public class ItemTrendRowVM
{
    public string ItemName      { get; set; } = "";
    public string LastMonthText { get; set; } = "";
    public string ThisMonthText { get; set; } = "";
    public string NextMonthText { get; set; } = "";
    public string Trend         { get; set; } = "";
    public Brush  TrendBg       { get; set; } = Brushes.Transparent;
    public Brush  TrendFg       { get; set; } = Brushes.Black;
}

public class DayBarVM
{
    public string DayName { get; set; } = "";
    public int    Count   { get; set; }
    public double BarPx   { get; set; }
}

public class HourBarVM
{
    public string HourLabel { get; set; } = "";
    public int    Count     { get; set; }
    public double BarPx     { get; set; }
}

// ── View ──────────────────────────────────────────────────────
public partial class PredictionsView : UserControl
{
    private readonly PredictionService _service = new();

    public PredictionsView()
    {
        InitializeComponent();
        Loaded += (s, e) => LoadAll();
    }

    private void LoadAll()
    {
        LoadSalesForecast();
        LoadWarnings();
        LoadItemTrends();
        LoadPeakPeriods();
    }

    // ── 1. Sales Forecast ─────────────────────────────────────
    private void LoadSalesForecast()
    {
        try
        {
            var p = _service.PredictNextMonthSales();
            NextMonthName.Text        = p.MonthName;
            PredictedSalesLabel.Text  = $"Rs. {p.PredictedAmount:N0}";
            LastMonthSalesLabel.Text  = $"Rs. {p.LastMonthAmount:N0}";
            ConfidenceLabel.Text      = $"Confidence: {p.Confidence}";
            SalesTrendIcon.Text       = p.IsIncrease ? "↑" : "↓";
            SalesTrendIcon.Foreground = p.IsIncrease
                ? new SolidColorBrush(Color.FromRgb(22, 163, 74))
                : new SolidColorBrush(Color.FromRgb(220, 38, 38));
            SalesTrendLabel.Text = $" {p.ChangePercent}% vs last month";
        }
        catch
        {
            PredictedSalesLabel.Text = "Insufficient data";
        }
    }

    // ── 2. Doctor Warnings ────────────────────────────────────
    private void LoadWarnings()
    {
        var warnings = _service.GetDoctorBalanceWarnings();
        if (warnings.Count == 0)
        {
            WarningsList.Visibility  = Visibility.Collapsed;
            NoWarningsMsg.Visibility = Visibility.Visible;
            return;
        }

        NoWarningsMsg.Visibility = Visibility.Collapsed;
        WarningsList.Visibility  = Visibility.Visible;
        WarningsList.ItemsSource  = warnings.Select(w => new WarningRowVM
        {
            DoctorName   = w.DoctorName,
            BalanceText  = $"Balance: Rs. {w.CurrentBalance:N0}  |  Avg daily: Rs. {w.AvgDailySpend:N0}",
            DaysText     = w.DaysUntilEmpty >= 999
                           ? "Balance OK"
                           : w.DaysUntilEmpty == 0
                             ? "Overdue"
                             : $"{w.DaysUntilEmpty} days left",
            WarningLevel = w.WarningLevel
        }).ToList();
    }

    // ── 3. Item Trends ────────────────────────────────────────
    private void LoadItemTrends()
    {
        var trends = _service.GetItemTrends();
        if (trends.Count == 0)
        {
            ItemTrendsList.Visibility = Visibility.Collapsed;
            NoTrendsMsg.Visibility    = Visibility.Visible;
            return;
        }

        NoTrendsMsg.Visibility    = Visibility.Collapsed;
        ItemTrendsList.Visibility = Visibility.Visible;

        var green  = new SolidColorBrush(Color.FromRgb(220, 252, 231));
        var red    = new SolidColorBrush(Color.FromRgb(254, 226, 226));
        var yellow = new SolidColorBrush(Color.FromRgb(254, 249, 195));
        var gFg    = new SolidColorBrush(Color.FromRgb(22, 101, 52));
        var rFg    = new SolidColorBrush(Color.FromRgb(153, 27, 27));
        var yFg    = new SolidColorBrush(Color.FromRgb(146, 64, 14));

        // Need last month qty — re-fetch via service (already embedded in trend)
        ItemTrendsList.ItemsSource = trends.Select(t => new ItemTrendRowVM
        {
            ItemName      = t.ItemName,
            LastMonthText = "—",   // last month baked into trend calc
            ThisMonthText = $"{t.CurrentMonthQty} units",
            NextMonthText = $"~{t.PredictedNextQty} units",
            Trend         = t.Trend,
            TrendBg       = t.Trend.StartsWith("Rising")  ? green
                          : t.Trend.StartsWith("Falling") ? red : yellow,
            TrendFg       = t.Trend.StartsWith("Rising")  ? gFg
                          : t.Trend.StartsWith("Falling") ? rFg : yFg
        }).ToList();
    }

    // ── 4. Peak Periods ───────────────────────────────────────
    private void LoadPeakPeriods()
    {
        var peaks = _service.GetPeakPeriods();

        PeakDaySubtitle.Text  = $"Busiest day: {peaks.PeakDay} ({peaks.PeakDayOrders} orders)";
        PeakHourSubtitle.Text = $"Busiest hour: {peaks.PeakHour} ({peaks.PeakHourOrders} orders)";

        const double maxBarPx = 180.0;

        PeakDaysList.ItemsSource = peaks.DayCounts.Select(d => new DayBarVM
        {
            DayName = d.DayName,
            Count   = d.Count,
            BarPx   = d.BarRatio * maxBarPx
        }).ToList();

        PeakHoursList.ItemsSource = peaks.HourCounts.Select(h => new HourBarVM
        {
            HourLabel = h.HourLabel,
            Count     = h.Count,
            BarPx     = h.BarRatio * maxBarPx
        }).ToList();
    }
}
