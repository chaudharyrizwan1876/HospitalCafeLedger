using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class AddDoctorDialog : Window
{
    public string DoctorName { get; private set; } = "";
    public string Department { get; private set; } = "";
    public string Phone { get; private set; } = "";
    public decimal OpeningBalance { get; private set; }

    public AddDoctorDialog() { InitializeComponent(); }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Please enter doctor name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }
        DoctorName = NameBox.Text.Trim();
        Department = (DeptBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        Phone = PhoneBox.Text.Trim();
        decimal.TryParse(BalanceBox.Text, out var bal);
        OpeningBalance = bal;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
