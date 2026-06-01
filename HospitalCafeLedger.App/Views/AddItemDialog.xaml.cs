using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class AddItemDialog : Window
{
    public string ItemName { get; private set; } = "";
    public decimal ItemPrice { get; private set; }
    public string ItemCategory { get; private set; } = "";

    public AddItemDialog() { InitializeComponent(); }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemNameBox.Text))
        {
            MessageBox.Show("Please enter item name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ItemNameBox.Focus();
            return;
        }
        if (!decimal.TryParse(ItemPriceBox.Text, out var price) || price < 0)
        {
            MessageBox.Show("Please enter a valid price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ItemPriceBox.Focus();
            return;
        }
        ItemName = ItemNameBox.Text.Trim();
        ItemPrice = price;
        ItemCategory = (CategoryBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
