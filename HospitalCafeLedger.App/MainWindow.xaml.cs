using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HospitalCafeLedger.App;

public partial class MainWindow : Window
{
    private readonly SolidColorBrush _activeColor  = new(Color.FromRgb(14, 77, 181));   // #0E4DB5
    private readonly SolidColorBrush _normalColor  = new(Colors.Transparent);
    private readonly SolidColorBrush _activeFg     = new(Colors.White);
    private readonly SolidColorBrush _normalFg     = new(Color.FromRgb(176, 196, 222)); // #B0C4DE

    private Button? _activeBtn;

    // Lazy view instances - created only when first navigated to
    private Views.DashboardView?  _dashboard;
    private Views.DoctorsView?    _doctors;
    private Views.ItemsView?      _items;
    private Views.BillingView?    _billing;
    private Views.PaymentsView?   _payments;
    private Views.LedgerView?     _ledger;
    private Views.ReportsView?    _reports;
    private Views.BackupView?     _backup;
    private Views.SettingsView?   _settings;

    public MainWindow()
    {
        InitializeComponent();
        _activeBtn = BtnDashboard;
        // Load dashboard immediately on startup
        Loaded += (s, e) => ShowSection("Dashboard");
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        // Reset previous
        if (_activeBtn != null)
        {
            _activeBtn.Background = _normalColor;
            _activeBtn.Foreground = _normalFg;
        }

        // Activate new
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
                "Reports"   => _reports   ??= new Views.ReportsView(),
                "Backup"    => _backup    ??= new Views.BackupView(),
                "Settings"  => _settings  ??= new Views.SettingsView(),
                _           => _dashboard ??= new Views.DashboardView(),
            };

            ContentArea.Children.Add(view);
        }
        catch (Exception ex)
        {
            // Show a friendly error panel instead of crashing
            var msg = new TextBlock
            {
                Text = $"Could not load '{tag}' view.\n\n{GetFullMessage(ex)}",
                Margin = new Thickness(30),
                FontSize = 14,
                Foreground = Brushes.Red,
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
        var current = ex;
        while (current != null)
        {
            sb.AppendLine(current.Message);
            current = current.InnerException;
        }
        return sb.ToString();
    }
}
