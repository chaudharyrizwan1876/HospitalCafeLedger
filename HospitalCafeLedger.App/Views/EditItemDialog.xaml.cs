using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class EditItemDialog : Window
{
    public string  ItemName     { get; private set; } = "";
    public string  ItemCategory { get; private set; } = "";
    public decimal ItemPrice    { get; private set; }
    public bool    ItemIsActive { get; private set; }

    public EditItemDialog(ItemViewModel item)
    {
        InitializeComponent();

        ItemNameBox.Text  = item.Name;
        ItemPriceBox.Text = item.Price.ToString();
        SubtitleText.Text = $"Editing: {item.Name} (ID: {item.Id})";

        foreach (ComboBoxItem ci in CategoryBox.Items)
        {
            if (ci.Content?.ToString() == item.Category)
            {
                CategoryBox.SelectedItem = ci;
                break;
            }
        }
        if (CategoryBox.SelectedIndex < 0)
            CategoryBox.SelectedIndex = 0;

        StatusBox.SelectedIndex = item.IsActive ? 0 : 1;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemNameBox.Text))
        {
            MessageBox.Show("Item name cannot be empty.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            ItemNameBox.Focus();
            return;
        }
        if (!decimal.TryParse(ItemPriceBox.Text, out var price) || price < 0)
        {
            MessageBox.Show("Please enter a valid price.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            ItemPriceBox.Focus();
            return;
        }
        ItemName     = ItemNameBox.Text.Trim();
        ItemCategory = (CategoryBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        ItemPrice    = price;
        ItemIsActive = StatusBox.SelectedIndex == 0;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
