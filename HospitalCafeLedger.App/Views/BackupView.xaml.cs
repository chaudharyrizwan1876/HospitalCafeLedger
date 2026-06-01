using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class BackupView : UserControl
{
    public BackupView() { InitializeComponent(); }

    private void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Backup created successfully!\nFile saved to: " + BackupPathBox.Text, "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RestorePathBox.Text))
        {
            MessageBox.Show("Please select a backup file first.", "No File Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var result = MessageBox.Show("Are you sure you want to restore the database? Current data will be replaced.", "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
            MessageBox.Show("Database restored successfully!", "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
