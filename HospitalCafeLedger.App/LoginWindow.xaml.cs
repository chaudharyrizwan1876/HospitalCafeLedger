using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HospitalCafeLedger.App;

public partial class LoginWindow : Window
{
    private const string ValidEmail    = "admin@cafeledger.com";
    private const string ValidPassword = "Admin@cafe";
    private const string CredsFile     = "remember.dat";

    public LoginWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;

        PasswordBox.PasswordChanged += (_, _) =>
            PassPlaceholder.Visibility =
                PasswordBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;

        EmailBox.TextChanged += (_, _) =>
            EmailPlaceholder.Visibility =
                string.IsNullOrEmpty(EmailBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ((Storyboard)FindResource("FadeIn")).Begin();
        LoadRemembered();
    }

    // ── Remember Me ───────────────────────────────────────────
    private void LoadRemembered()
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CredsFile);
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) return;
            EmailBox.Text        = lines[0];
            PasswordBox.Password = lines[1];
            RememberMe.IsChecked = true;
            EmailPlaceholder.Visibility = Visibility.Collapsed;
            PassPlaceholder.Visibility  = Visibility.Collapsed;
        }
        catch { /* ignore */ }
    }

    private void SaveRemembered()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CredsFile);
        if (RememberMe.IsChecked == true)
            File.WriteAllLines(path, new[] { EmailBox.Text.Trim(), PasswordBox.Password });
        else if (File.Exists(path))
            File.Delete(path);
    }

    // ── Login ─────────────────────────────────────────────────
    private void Login_Click(object sender, RoutedEventArgs e) => TryLogin();

    private void Password_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryLogin();
    }

    private void TryLogin()
    {
        // Reset UI state
        EmailError.Visibility   = Visibility.Collapsed;
        PassError.Visibility    = Visibility.Collapsed;
        ErrorBanner.Visibility  = Visibility.Collapsed;
        SetBorderNormal(EmailBorder);
        SetBorderNormal(PassBorder);

        var email = EmailBox.Text.Trim();
        var pass  = PasswordBox.Password;
        bool valid = true;

        if (string.IsNullOrWhiteSpace(email))
        {
            EmailError.Text       = "Email is required";
            EmailError.Visibility = Visibility.Visible;
            SetBorderError(EmailBorder);
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(pass))
        {
            PassError.Text       = "Password is required";
            PassError.Visibility = Visibility.Visible;
            SetBorderError(PassBorder);
            valid = false;
        }

        if (!valid) return;

        if (email != ValidEmail || pass != ValidPassword)
        {
            ErrorMsg.Text          = "❌  Invalid email or password. Please try again.";
            ErrorBanner.Visibility = Visibility.Visible;
            ShakeCard();
            return;
        }

        SaveRemembered();
        LoginBtn.IsEnabled = false;
        LoginBtn.Content   = "Signing in...";

        var fadeOut = (Storyboard)FindResource("LoginAnim");
        fadeOut.Completed += (_, _) =>
        {
            var main = new MainWindow();
            main.Show();
            Close();
        };
        fadeOut.Begin();
    }

    // ── Shake animation ───────────────────────────────────────
    private void ShakeCard()
    {
        // Ensure TranslateTransform exists
        if (CardBorder.RenderTransform is not TranslateTransform)
            CardBorder.RenderTransform = new TranslateTransform();

        var shake = new DoubleAnimationUsingKeyFrames();
        shake.KeyFrames.Add(new EasingDoubleKeyFrame(0,   KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
        shake.KeyFrames.Add(new EasingDoubleKeyFrame(10,  KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(60))));
        shake.KeyFrames.Add(new EasingDoubleKeyFrame(-10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120))));
        shake.KeyFrames.Add(new EasingDoubleKeyFrame(8,   KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))));
        shake.KeyFrames.Add(new EasingDoubleKeyFrame(-8,  KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(240))));
        shake.KeyFrames.Add(new EasingDoubleKeyFrame(0,   KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

        CardBorder.RenderTransform.BeginAnimation(TranslateTransform.XProperty, shake);
    }

    // ── Border helpers ────────────────────────────────────────
    private static readonly SolidColorBrush NormalBrush =
        new(Color.FromRgb(226, 232, 240));
    private static readonly SolidColorBrush ErrorBrush =
        new(Color.FromRgb(220, 38, 38));
    private static readonly SolidColorBrush FocusBrush =
        new(Color.FromRgb(14, 77, 181));

    private static void SetBorderNormal(Border b)
    {
        b.BorderBrush     = NormalBrush;
        b.BorderThickness = new Thickness(1);
    }

    private static void SetBorderError(Border b)
    {
        b.BorderBrush     = ErrorBrush;
        b.BorderThickness = new Thickness(1);
    }

    // ── Focus / LostFocus ─────────────────────────────────────
    private void Field_GotFocus(object sender, RoutedEventArgs e)
    {
        var b = sender is TextBox ? EmailBorder : PassBorder;
        b.BorderBrush     = FocusBrush;
        b.BorderThickness = new Thickness(2);
    }

    private void Field_LostFocus(object sender, RoutedEventArgs e)
    {
        SetBorderNormal(EmailBorder);
        SetBorderNormal(PassBorder);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
        => Application.Current.Shutdown();
}
