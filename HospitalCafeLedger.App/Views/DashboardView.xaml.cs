using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

public class TopItemBarVM
{
    public string Name          { get; set; } = "";
    public int    OrderCount    { get; set; }
    public double BarPixelWidth { get; set; }
}

public partial class DashboardView : UserControl
{
    private readonly DashboardService _service = new();

    public DashboardView()
    {
        InitializeComponent();
        Loaded += (s, e) => LoadDashboard();
    }

    private void LoadDashboard()
    {
        var now   = DateTime.Now;
        TodayDateLabel.Text = now.ToString("dddd, dd MMMM yyyy");
        SubtitleText.Text   = $"Welcome back — {now:hh:mm tt}";
        ChartMonthLabel.Text = now.ToString("MMMM yyyy");

        // ── Stat cards ──────────────────────────────────────
        var stats = _service.GetStats();
        TodaySalesLabel.Text    = $"Rs. {stats.TodaySales:N0}";
        TotalPendingLabel.Text  = $"Rs. {stats.TotalPending:N0}";
        TotalDoctorsLabel.Text  = stats.TotalDoctors.ToString();
        TodayOrdersLabel.Text   = stats.TodayOrders.ToString();
        PendingAmountLabel.Text = $"Rs. {stats.TotalPending:N0}";
        PendingDoctorsLabel.Text = stats.DoctorsWithPending.ToString();
        AdvanceCreditLabel.Text  = $"Rs. {stats.TotalAdvanceCredit:N0}";

        // ── Top Items ────────────────────────────────────────
        var topItems = _service.GetTopItems(now.Month, now.Year);
        if (topItems.Count == 0)
        {
            NoItemsMsg.Visibility  = Visibility.Visible;
            TopItemsList.Visibility = Visibility.Collapsed;
        }
        else
        {
            NoItemsMsg.Visibility  = Visibility.Collapsed;
            TopItemsList.Visibility = Visibility.Visible;
            // Bar pixel width based on available space (~230px max)
            TopItemsList.ItemsSource = topItems.Select(t => new TopItemBarVM
            {
                Name          = t.Name,
                OrderCount    = t.OrderCount,
                BarPixelWidth = t.BarWidth * 230
            }).ToList();
        }

        // ── Recent Transactions ──────────────────────────────
        var txns = _service.GetRecentTransactions();
        if (txns.Count == 0)
        {
            NoTxMsg.Visibility     = Visibility.Visible;
            RecentTxList.Visibility = Visibility.Collapsed;
        }
        else
        {
            NoTxMsg.Visibility     = Visibility.Collapsed;
            RecentTxList.Visibility = Visibility.Visible;
            RecentTxList.ItemsSource = txns;
        }

        // ── Chart (drawn after layout) ───────────────────────
        SalesChart.Loaded -= Chart_Loaded;
        SalesChart.Loaded += Chart_Loaded;
        if (SalesChart.ActualWidth > 0)
            DrawChart(now.Month, now.Year);
    }

    private void Chart_Loaded(object sender, RoutedEventArgs e)
    {
        DrawChart(DateTime.Now.Month, DateTime.Now.Year);
    }

    private void DrawChart(int month, int year)
    {
        SalesChart.Children.Clear();

        var data    = _service.GetMonthlyDailyAmounts(month, year);
        double w    = SalesChart.ActualWidth;
        double h    = SalesChart.ActualHeight;
        if (w <= 0 || h <= 0 || data.Count == 0) return;

        double maxVal = (double)(data.Max(d => d.Amount));
        if (maxVal == 0) maxVal = 1;

        double padL = 46, padR = 14, padT = 12, padB = 30;
        double chartW = w - padL - padR;
        double chartH = h - padT - padB;

        // Y-axis grid lines + labels (5 lines)
        for (int i = 0; i <= 4; i++)
        {
            double yVal = maxVal * i / 4;
            double y    = padT + chartH - (chartH * i / 4);

            var line = new Line
            {
                X1 = padL, X2 = w - padR,
                Y1 = y,    Y2 = y,
                Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                StrokeThickness = 1
            };
            SalesChart.Children.Add(line);

            var lbl = new TextBlock
            {
                Text       = yVal >= 1000 ? $"{yVal/1000:0.#}K" : $"{yVal:0}",
                FontSize   = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
            };
            Canvas.SetLeft(lbl, 0);
            Canvas.SetTop(lbl, y - 8);
            SalesChart.Children.Add(lbl);
        }

        // X-axis day labels (every 5 days)
        for (int i = 0; i < data.Count; i++)
        {
            if ((data[i].Day % 5 == 0) || data[i].Day == 1)
            {
                double x = padL + (i / (double)(data.Count - 1)) * chartW;
                var lbl = new TextBlock
                {
                    Text       = data[i].Day.ToString(),
                    FontSize   = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
                };
                Canvas.SetLeft(lbl, x - 5);
                Canvas.SetTop(lbl, h - padB + 4);
                SalesChart.Children.Add(lbl);
            }
        }

        // Build point list
        var points = data.Select((d, i) =>
        {
            double x = padL + (i / (double)(data.Count - 1)) * chartW;
            double y = padT + chartH - (chartH * (double)d.Amount / maxVal);
            return new System.Windows.Point(x, y);
        }).ToList();

        // Filled area under line
        var poly = new Polygon
        {
            Fill    = new LinearGradientBrush(
                        Color.FromArgb(60, 14, 77, 181),
                        Color.FromArgb(5,  14, 77, 181),
                        90),
            Stroke  = Brushes.Transparent
        };
        poly.Points.Add(new System.Windows.Point(padL, padT + chartH));
        foreach (var p in points) poly.Points.Add(p);
        poly.Points.Add(new System.Windows.Point(w - padR, padT + chartH));
        SalesChart.Children.Add(poly);

        // Line segments
        for (int i = 0; i < points.Count - 1; i++)
        {
            SalesChart.Children.Add(new Line
            {
                X1 = points[i].X,     Y1 = points[i].Y,
                X2 = points[i+1].X,   Y2 = points[i+1].Y,
                Stroke          = new SolidColorBrush(Color.FromRgb(14, 77, 181)),
                StrokeThickness = 2.5
            });
        }

        // Dots on data points that have sales
        foreach (var (pt, d) in points.Zip(data))
        {
            if (d.Amount == 0) continue;
            var dot = new Ellipse
            {
                Width  = 7, Height = 7,
                Fill   = new SolidColorBrush(Color.FromRgb(14, 77, 181)),
                Stroke = Brushes.White, StrokeThickness = 1.5
            };
            Canvas.SetLeft(dot, pt.X - 3.5);
            Canvas.SetTop(dot,  pt.Y - 3.5);
            SalesChart.Children.Add(dot);
        }
    }
}
