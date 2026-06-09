using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class EditDoctorDialog : Window
{
    public string DoctorName { get; private set; } = "";
    public string Department { get; private set; } = "";
    public string Phone { get; private set; } = "";
    public decimal OpeningBalance { get; private set; }
    public bool    DoctorIsActive    { get; private set; }

    public EditDoctorDialog(DoctorViewModel doctor)
    {
        InitializeComponent();

        // Pre-fill all fields
        NameBox.Text = doctor.Name;
        PhoneBox.Text = doctor.PhoneNumber;
        BalanceBox.Text = doctor.OpeningBalance.ToString();
        SubtitleText.Text = $"Editing: {doctor.Name} (ID: {doctor.Id})";

        // Set department ComboBox
        foreach (ComboBoxItem item in DeptBox.Items)
        {
            if (item.Content?.ToString() == doctor.Department)
            {
                DeptBox.SelectedItem = item;
                break;
            }
        }
        if (DeptBox.SelectedIndex < 0) DeptBox.SelectedIndex = 0;

        // Set status ComboBox
        StatusBox.SelectedIndex = doctor.IsActive ? 0 : 1;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Doctor name cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }
        DoctorName = NameBox.Text.Trim();
        Department = (DeptBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        Phone = PhoneBox.Text.Trim();
        decimal.TryParse(BalanceBox.Text, out var bal);
        OpeningBalance = bal;
        DoctorIsActive = StatusBox.SelectedIndex == 0;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
