using System.Windows;
using System.Windows.Threading;
using HospitalCafeLedger.Data;

namespace HospitalCafeLedger.App;

public partial class App : Application
{
    public App()
    {
        // Ensure SQLite DB and tables are created on first run
        using var db = new AppDbContext();
        db.Database.EnsureCreated();

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show(BuildMessage(ex), "Unhandled Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, e) =>
        {
            MessageBox.Show(BuildMessage(e.Exception), "Runtime Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }

    private static string BuildMessage(Exception? ex)
    {
        if (ex == null) return "Unknown error occurred.";
        var sb = new System.Text.StringBuilder();
        var current = ex;
        int depth = 0;
        while (current != null && depth < 5)
        {
            if (depth > 0) sb.AppendLine("\n--- Inner Exception ---");
            sb.AppendLine(current.GetType().FullName);
            sb.AppendLine(current.Message);
            current = current.InnerException;
            depth++;
        }
        return sb.ToString();
    }
}
