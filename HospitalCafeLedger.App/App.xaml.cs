using System.Windows;
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
            MessageBox.Show(BuildMessage(ex), "Unhandled Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, e) =>
        {
            MessageBox.Show(BuildMessage(e.Exception), "Runtime Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }

    private static string BuildMessage(Exception? ex)
    {
        if (ex == null) return "Unknown error.";
        var sb = new System.Text.StringBuilder();
        var cur = ex;
        int depth = 0;
        while (cur != null && depth < 5)
        {
            if (depth > 0) sb.AppendLine("\n--- Inner ---");
            sb.AppendLine(cur.GetType().FullName);
            sb.AppendLine(cur.Message);
            cur = cur.InnerException;
            depth++;
        }
        return sb.ToString();
    }
}
