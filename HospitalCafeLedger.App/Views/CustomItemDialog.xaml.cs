using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class CustomItemDialog : Window
{
    public string  ItemName  { get; private set; } = "";
    public decimal ItemPrice { get; private set; }

    public CustomItemDialog() { InitializeComponent(); }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemNameBox.Text))
        {
            MessageBox.Show("Please enter item name.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            ItemNameBox.Focus();
            return;
        }
        if (!decimal.TryParse(ItemPriceBox.Text, out var price) || price <= 0)
        {
            MessageBox.Show("Please enter a valid price greater than 0.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            ItemPriceBox.Focus();
            return;
        }
        ItemName  = ItemNameBox.Text.Trim();
        ItemPrice = price;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
