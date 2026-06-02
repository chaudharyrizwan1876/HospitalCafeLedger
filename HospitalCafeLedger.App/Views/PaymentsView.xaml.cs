using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HospitalCafeLedger.Models;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

public class PaymentRowVM
{
    public string DateDisplay   { get; set; } = "";
    public string AmountDisplay { get; set; } = "";
    public string Notes         { get; set; } = "";
}

public class PayDoctorItem
{
    public int    Id      { get; set; }
    public string Display { get; set; } = "";
}

public partial class PaymentsView : UserControl
{
    private readonly DoctorService  _doctorService  = new();
    private readonly PaymentService _paymentService = new();

    private List<Doctor> _allDoctors    = new();
    private Doctor?      _selectedDoctor;

    public PaymentsView()
    {
        InitializeComponent();
        Loaded += (s, e) => Initialize();
    }

    private void Initialize()
    {
        PayDatePicker.SelectedDate = DateTime.Today;
        LoadDoctors();
    }

    // ── Doctors ───────────────────────────────────────────────
    private void LoadDoctors()
    {
        _allDoctors = _doctorService.GetAll().Where(d => d.IsActive).ToList();
        BindDoctors(_allDoctors);
    }

    private void BindDoctors(List<Doctor> list)
    {
        PayDoctorList.ItemsSource       = list
            .Select(d => new PayDoctorItem
            {
                Id      = d.Id,
                Display = $"D{d.Id:D3} - {d.Name}"
            }).ToList();
        PayDoctorList.DisplayMemberPath = "Display";
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
        if (PayDoctorList.SelectedItem is not PayDoctorItem sel) return;
        _selectedDoctor = _allDoctors.FirstOrDefault(d => d.Id == sel.Id);
        if (_selectedDoctor != null)
            RefreshPanel();
    }

    // ── Refresh Panel ─────────────────────────────────────────
    private void RefreshPanel()
    {
        if (_selectedDoctor == null) return;

        var s = _paymentService.GetSummary(_selectedDoctor.Id, _selectedDoctor.OpeningBalance);

        // Doctor labels
        DoctorInfoLabel.Text     = $"{_selectedDoctor.Name}  (D{_selectedDoctor.Id:D3})";
        DeptLabel.Text           = _selectedDoctor.Department;

        // Wallet figures — center panel
        OpeningBalLabel.Text     = $"Rs. {s.OpeningBalance:N0}";
        CashDepositsLabel.Text   = $"Rs. {s.TotalDeposits - s.OpeningBalance:N0}";
        TotalDepositedLabel.Text = $"Rs. {s.TotalDeposits:N0}";
        TotalOrdersLabel.Text    = $"Rs. {s.TotalOrders:N0}";

        // Available balance color — green if positive, red if debt
        var balColor = s.IsInDebt
            ? new SolidColorBrush(Color.FromRgb(220, 38, 38))   // red
            : new SolidColorBrush(Color.FromRgb(22, 163, 74));  // green

        AvailableBalanceLabel.Text       = s.IsInDebt
            ? $"− Rs. {Math.Abs(s.AvailableBalance):N0}"
            : $"Rs. {s.AvailableBalance:N0}";
        AvailableBalanceLabel.Foreground = balColor;
        BalanceTitle.Text                = s.IsInDebt ? "Amount Owed" : "Available Balance";
        BalanceTitle.Foreground          = balColor;

        // Debt banner
        DebtBanner.Visibility = s.IsInDebt ? Visibility.Visible : Visibility.Collapsed;
        if (s.IsInDebt)
            DebtAmountRun.Text = $"Doctor owes Rs. {Math.Abs(s.AvailableBalance):N0} — deposits are less than orders placed";

        // Right summary card
        SummaryOpening.Text   = $"Rs. {s.OpeningBalance:N0}";
        SummaryCash.Text      = $"Rs. {s.TotalDeposits - s.OpeningBalance:N0}";
        SummaryOrders.Text    = $"Rs. {s.TotalOrders:N0}";
        SummaryAvailable.Text = s.IsInDebt
            ? $"− Rs. {Math.Abs(s.AvailableBalance):N0}"
            : $"Rs. {s.AvailableBalance:N0}";
        SummaryAvailable.Foreground = balColor;
        SummaryBalTitle.Foreground  = balColor;
        SummaryBalTitle.Text        = s.IsInDebt ? "Amount Owed" : "Available";

        LoadPaymentHistory();
    }

    private void LoadPaymentHistory()
    {
        if (_selectedDoctor == null) return;

        var history = _paymentService.GetHistory(_selectedDoctor.Id);
        NoPaymentsMsg.Visibility       = history.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        PaymentHistoryList.ItemsSource = history
            .Select(p => new PaymentRowVM
            {
                DateDisplay   = p.PaymentDate.ToString("dd MMM yyyy"),
                AmountDisplay = $"Rs. {p.Amount:N0}",
                Notes         = string.IsNullOrWhiteSpace(p.Notes) ? "—" : p.Notes
            })
            .ToList();
    }

    // ── Save Deposit ──────────────────────────────────────────
    private void SavePayment_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDoctor == null)
        {
            MessageBox.Show("Please select a doctor first.", "No Doctor",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(PayAmountBox.Text) ||
            !decimal.TryParse(PayAmountBox.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show("Please enter a valid amount greater than 0.", "Invalid Amount",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            PayAmountBox.Focus();
            return;
        }

        if (PayDatePicker.SelectedDate == null)
        {
            MessageBox.Show("Please select a date.", "No Date",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _paymentService.AddPayment(new Payment
        {
            DoctorId    = _selectedDoctor.Id,
            Amount      = amount,
            PaymentDate = PayDatePicker.SelectedDate.Value,
            Notes       = string.IsNullOrWhiteSpace(PayNoteBox.Text)
                          ? "Cash Deposit" : PayNoteBox.Text.Trim()
        });

        // Get updated balance for message
        var s = _paymentService.GetSummary(_selectedDoctor.Id, _selectedDoctor.OpeningBalance);
        MessageBox.Show(
            $"Deposit of Rs. {amount:N0} saved!\n" +
            $"Doctor: {_selectedDoctor.Name}\n\n" +
            $"New Available Balance: Rs. {s.AvailableBalance:N0}",
            "Deposit Saved", MessageBoxButton.OK, MessageBoxImage.Information);

        // Reset form
        PayAmountBox.Text          = "";
        PayNoteBox.Text            = "Cash Deposit";
        PayDatePicker.SelectedDate = DateTime.Today;

        RefreshPanel();
    }
}
