using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HospitalCafeLedger.App;

public partial class MainWindow : Window
{
    private readonly SolidColorBrush _activeColor = new(Color.FromRgb(14, 77, 181));
    private readonly SolidColorBrush _normalColor = new(Colors.Transparent);
    private readonly SolidColorBrush _activeFg    = new(Colors.White);
    private readonly SolidColorBrush _normalFg    = new(Color.FromRgb(176, 196, 222));

    private Button? _activeBtn;

    private Views.DashboardView?    _dashboard;
    private Views.DoctorsView?      _doctors;
    private Views.ItemsView?        _items;
    private Views.BillingView?      _billing;
    private Views.PaymentsView?     _payments;
    private Views.LedgerView?       _ledger;
    private Views.ReportsView?      _reports;
    private Views.PredictionsView?  _predictions;
    private Views.BackupView?       _backup;

    public MainWindow()
    {
        InitializeComponent();
        _activeBtn = BtnDashboard;
        Loaded += (s, e) => ShowSection("Dashboard");
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        if (_activeBtn != null)
        {
            _activeBtn.Background = _normalColor;
            _activeBtn.Foreground = _normalFg;
        }

        btn.Background = _activeColor;
        btn.Foreground = _activeFg;
        _activeBtn = btn;

        ShowSection(btn.Tag?.ToString() ?? "Dashboard");
    }

    private void ShowSection(string tag)
    {
        ContentArea.Children.Clear();
        try
        {
            UIElement view = tag switch
            {
                "Dashboard" => _dashboard ??= new Views.DashboardView(),
                "Doctors"   => _doctors   ??= new Views.DoctorsView(),
                "Items"     => _items     ??= new Views.ItemsView(),
                "Billing"   => _billing   ??= new Views.BillingView(),
                "Payments"  => _payments  ??= new Views.PaymentsView(),
                "Ledger"    => _ledger    ??= new Views.LedgerView(),
                "Reports"     => _reports     ??= new Views.ReportsView(),
                "Predictions" => _predictions ??= new Views.PredictionsView(),
                "Backup"      => _backup      ??= new Views.BackupView(),
                _           => _dashboard ??= new Views.DashboardView(),
            };
            ContentArea.Children.Add(view);
        }
        catch (Exception ex)
        {
            var msg = new TextBlock
            {
                Text         = $"Could not load '{tag}' view.\n\n{GetFullMessage(ex)}",
                Margin       = new Thickness(30),
                FontSize     = 14,
                Foreground   = Brushes.Red,
                TextWrapping = TextWrapping.Wrap
            };
            ContentArea.Children.Add(msg);
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var r = MessageBox.Show("Are you sure you want to logout?",
                    "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r == MessageBoxResult.Yes) Application.Current.Shutdown();
    }

    private static string GetFullMessage(Exception? ex)
    {
        var sb = new System.Text.StringBuilder();
        var cur = ex;
        while (cur != null) { sb.AppendLine(cur.Message); cur = cur.InnerException; }
        return sb.ToString();
    }
}
