using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

public class ReportRowVM
{
    public int    Serial              { get; set; }
    public string DoctorName         { get; set; } = "";
    public string OpeningBalDisplay  { get; set; } = "";
    public string TotalDepositDisplay { get; set; } = "";
    public string TotalOrdersDisplay { get; set; } = "";
    public string AvailableDisplay   { get; set; } = "";
    public Brush  BalanceColor       { get; set; } = Brushes.Black;
}

public partial class ReportsView : UserControl
{
    private readonly ReportService _reportService = new();
    private ReportResult? _lastResult;

    private static readonly string[] MonthNames =
    {
        "January","February","March","April","May","June",
        "July","August","September","October","November","December"
    };

    public ReportsView()
    {
        InitializeComponent();
        Loaded += (s, e) => Initialize();
    }

    private void Initialize()
    {
        // Month dropdown
        MonthCombo.ItemsSource   = MonthNames.Select((m, i) => new { Name = m, Index = i + 1 }).ToList();
        MonthCombo.DisplayMemberPath = "Name";
        MonthCombo.SelectedIndex = DateTime.Now.Month - 1;

        // Year dropdown — dynamic from DB + current year
        var years = _reportService.GetAvailableYears();
        YearCombo.ItemsSource   = years;
        YearCombo.SelectedItem  = DateTime.Now.Year;
        if (YearCombo.SelectedItem == null && years.Count > 0)
            YearCombo.SelectedIndex = 0;
    }

    private void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        if (MonthCombo.SelectedItem == null || YearCombo.SelectedItem == null)
        {
            MessageBox.Show("Please select both month and year.", "Selection Required",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        dynamic monthItem = MonthCombo.SelectedItem;
        int month = (int)monthItem.Index;
        int year  = (int)YearCombo.SelectedItem;

        _lastResult = _reportService.Generate(month, year);

        PeriodLabel.Text = $"Report: {_lastResult.PeriodLabel}";

        if (_lastResult.Rows.Count == 0)
        {
            EmptyReport.Visibility  = Visibility.Visible;
            GrandTotalRow.Visibility = Visibility.Collapsed;
            ReportTable.ItemsSource  = null;
            return;
        }

        EmptyReport.Visibility   = Visibility.Collapsed;
        GrandTotalRow.Visibility = Visibility.Visible;

        var green = new SolidColorBrush(Color.FromRgb(22, 163, 74));
        var red   = new SolidColorBrush(Color.FromRgb(220, 38, 38));

        ReportTable.ItemsSource = _lastResult.Rows
            .Select((r, i) => new ReportRowVM
            {
                Serial               = i + 1,
                DoctorName           = r.DoctorName,
                OpeningBalDisplay    = $"Rs. {r.OpeningBalance:N0}",
                TotalDepositDisplay  = $"Rs. {r.TotalDeposits:N0}",
                TotalOrdersDisplay   = r.TotalOrders == 0 ? "—" : $"Rs. {r.TotalOrders:N0}",
                AvailableDisplay     = r.IsInDebt
                                       ? $"− Rs. {Math.Abs(r.AvailableBalance):N0}"
                                       : $"Rs. {r.AvailableBalance:N0}",
                BalanceColor         = r.IsInDebt ? red : green
            }).ToList();

        // Grand totals
        GrandOpening.Text  = $"Rs. {_lastResult.GrandOpeningBalance:N0}";
        GrandDeposits.Text = $"Rs. {_lastResult.GrandTotalDeposits:N0}";
        GrandOrders.Text   = _lastResult.GrandTotalOrders == 0
                             ? "—" : $"Rs. {_lastResult.GrandTotalOrders:N0}";
        GrandBalance.Text  = _lastResult.GrandAvailableBalance < 0
                             ? $"− Rs. {Math.Abs(_lastResult.GrandAvailableBalance):N0}"
                             : $"Rs. {_lastResult.GrandAvailableBalance:N0}";
    }

    private void ExportPrint_Click(object sender, RoutedEventArgs e)
    {
        if (_lastResult == null || _lastResult.Rows.Count == 0)
        {
            MessageBox.Show("Please generate a report first.", "No Report",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Build CSV
        var sb = new StringBuilder();
        sb.AppendLine($"Hospital Cafe Ledger — Monthly Report");
        sb.AppendLine($"Period: {_lastResult.PeriodLabel}");
        sb.AppendLine($"Generated: {DateTime.Now:dd MMM yyyy  hh:mm tt}");
        sb.AppendLine();
        sb.AppendLine("#,Doctor,Opening Balance,Total Deposited,Month Orders,Available Balance");

        int serial = 1;
        foreach (var r in _lastResult.Rows)
        {
            var bal = r.AvailableBalance < 0
                ? $"-{Math.Abs(r.AvailableBalance):N0}"
                : $"{r.AvailableBalance:N0}";
            sb.AppendLine($"{serial++},{r.DoctorName},{r.OpeningBalance:N0},{r.TotalDeposits:N0},{r.TotalOrders:N0},{bal}");
        }

        sb.AppendLine();
        sb.AppendLine($"Grand Total,,{_lastResult.GrandOpeningBalance:N0},{_lastResult.GrandTotalDeposits:N0},{_lastResult.GrandTotalOrders:N0},{_lastResult.GrandAvailableBalance:N0}");

        // Save dialog
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            FileName    = $"Report_{_lastResult.PeriodLabel.Replace(" ", "_")}",
            DefaultExt  = ".csv",
            Filter      = "CSV File (*.csv)|*.csv|Text File (*.txt)|*.txt"
        };

        if (dlg.ShowDialog() == true)
        {
            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show(
                $"Report exported successfully!\nFile: {dlg.FileName}",
                "Export Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
