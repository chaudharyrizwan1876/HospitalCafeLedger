using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace HospitalCafeLedger.App.Views;

public partial class BackupView : UserControl
{
    private static string DbPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hospitalcafe.db");

    public BackupView()
    {
        InitializeComponent();
        BackupPathBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    // ── Browse backup destination — use SaveFileDialog as folder picker ──
    private void BrowseBackupFolder_Click(object sender, RoutedEventArgs e)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var dlg = new SaveFileDialog
        {
            Title      = "Choose backup destination and filename",
            FileName   = $"hospitalcafe_{timestamp}.db",
            DefaultExt = ".db",
            Filter     = "SQLite Database (*.db)|*.db"
        };

        // Start in current path if valid
        var currentDir = BackupPathBox.Text.Trim();
        if (Directory.Exists(currentDir))
            dlg.InitialDirectory = currentDir;

        if (dlg.ShowDialog() == true)
        {
            // Store only the folder part so label stays clean
            BackupPathBox.Text = Path.GetDirectoryName(dlg.FileName) ?? currentDir;
            // Remember full chosen path for immediate use
            _chosenBackupPath = dlg.FileName;
        }
    }

    private string? _chosenBackupPath;

    // ── Create Backup ─────────────────────────────────────────
    private void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(DbPath))
        {
            MessageBox.Show("Database file not found.\nMake sure the app has been used at least once.",
                "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // If user already chose a path via Browse, use it; otherwise build one
        string backupFile;
        if (!string.IsNullOrWhiteSpace(_chosenBackupPath))
        {
            backupFile      = _chosenBackupPath;
            _chosenBackupPath = null;
        }
        else
        {
            var folder = BackupPathBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show("Please select a valid backup folder first.", "Invalid Folder",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            backupFile = Path.Combine(folder, $"hospitalcafe_{ts}.db");
        }

        try
        {
            File.Copy(DbPath, backupFile, overwrite: true);
            MessageBox.Show(
                $"✅ Backup created successfully!\n\nSaved to:\n{backupFile}",
                "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Backup failed:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Browse restore file ───────────────────────────────────
    private void BrowseRestoreFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title       = "Select Backup File to Restore",
            Filter      = "SQLite Database (*.db)|*.db",
            Multiselect = false
        };
        if (dlg.ShowDialog() == true)
            RestorePathBox.Text = dlg.FileName;
    }

    // ── Restore Database ──────────────────────────────────────
    private void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        var backupFile = RestorePathBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(backupFile) || !File.Exists(backupFile))
        {
            MessageBox.Show("Please select a valid backup (.db) file first.", "No File Selected",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            "⚠  Are you sure you want to restore the database?\n\n" +
            "ALL current data will be permanently replaced.\n" +
            "This action CANNOT be undone.\n\n" +
            "Proceed with restore?",
            "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            File.Copy(backupFile, DbPath, overwrite: true);
            MessageBox.Show(
                "✅ Database restored successfully!\n\n" +
                "Please restart the application for changes to take full effect.",
                "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            RestorePathBox.Text = "";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Restore failed:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
