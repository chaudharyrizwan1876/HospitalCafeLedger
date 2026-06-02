using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HospitalCafeLedger.Models;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

// Row ViewModel
public class LedgerRowVM
{
    public string ItemName    { get; set; } = "";
    public int    Qty         { get; set; }
    public decimal Price      { get; set; }
    public decimal Total      => Qty * Price;
    public string PriceDisplay => $"{Price:N0}";
    public string TotalDisplay => $"{Total:N0}";
}

// Day group ViewModel
public class LedgerDayVM
{
    public string           DateHeader     { get; set; } = "";
    public List<LedgerRowVM> Rows          { get; set; } = new();
    public decimal          DayTotal       => Rows.Sum(r => r.Total);
    public string           DayTotalDisplay => $"Rs. {DayTotal:N0}";
}

public class LedgerDoctorItem
{
    public int    Id      { get; set; }
    public string Display { get; set; } = "";
}

public partial class LedgerView : UserControl
{
    private readonly DoctorService _doctorService = new();
    private readonly LedgerService _ledgerService = new();

    private List<Doctor> _allDoctors    = new();
    private Doctor?      _selectedDoctor;

    public LedgerView()
    {
        InitializeComponent();
        Loaded += (s, e) => Initialize();
    }

    private void Initialize() => LoadDoctors();

    // ── Doctors ───────────────────────────────────────────────
    private void LoadDoctors()
    {
        _allDoctors = _doctorService.GetAll();
        BindDoctors(_allDoctors);
    }

    private void BindDoctors(List<Doctor> list)
    {
        LedgerDoctorList.ItemsSource = list
            .Select(d => new LedgerDoctorItem
            {
                Id      = d.Id,
                Display = $"D{d.Id:D3} - {d.Name}"
            }).ToList();
        LedgerDoctorList.DisplayMemberPath = "Display";
    }

    private void DoctorSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = DoctorSearchBox.Text.Trim();
        if (DoctorSearchPlaceholder != null)
            DoctorSearchPlaceholder.Visibility =
                string.IsNullOrEmpty(q) ? Visibility.Visible : Visibility.Collapsed;

        var filtered = string.IsNullOrEmpty(q)
            ? _allDoctors
            : _allDoctors.Where(d =>
                d.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.Id.ToString().Contains(q)).ToList();
        BindDoctors(filtered);
    }

    private void DoctorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LedgerDoctorList.SelectedItem is not LedgerDoctorItem sel) return;
        _selectedDoctor = _allDoctors.FirstOrDefault(d => d.Id == sel.Id);
        if (_selectedDoctor != null) RefreshLedger();
    }

    // ── Ledger ────────────────────────────────────────────────
    private void RefreshLedger()
    {
        if (_selectedDoctor == null) return;

        var summary = _ledgerService.GetLedger(_selectedDoctor.Id, _selectedDoctor.OpeningBalance);

        // Doctor info
        LedgerDoctorLabel.Text  = $"{_selectedDoctor.Name}  (D{_selectedDoctor.Id:D3})";
        LedgerOpeningLabel.Text = $"Rs. {summary.OpeningBalance:N0}";

        // Balance figures
        LedgerTotalDepositLabel.Text = $"Rs. {summary.TotalDeposits:N0}";
        LedgerTotalOrdersLabel.Text  = $"Rs. {summary.TotalOrders:N0}";

        var balColor = summary.IsInDebt
            ? new SolidColorBrush(Color.FromRgb(220, 38, 38))
            : new SolidColorBrush(Color.FromRgb(22, 163, 74));

        LedgerBalanceLabel.Text       = summary.IsInDebt
            ? $"− Rs. {Math.Abs(summary.AvailableBalance):N0}"
            : $"Rs. {summary.AvailableBalance:N0}";
        LedgerBalanceLabel.Foreground = balColor;
        LedgerBalTitle.Text           = summary.IsInDebt ? "Amount Owed" : "Available Balance";
        LedgerBalTitle.Foreground     = balColor;

        // Bind day groups
        bool hasData = summary.DayGroups.Count > 0;
        EmptyLedger.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;

        LedgerRows.ItemsSource = summary.DayGroups
            .Select(g => new LedgerDayVM
            {
                DateHeader = g.Date.ToString("dd MMMM yyyy"),
                Rows = g.Rows.Select(r => new LedgerRowVM
                {
                    ItemName = r.ItemName,
                    Qty      = r.Qty,
                    Price    = r.Price
                }).ToList()
            }).ToList();
    }
}
