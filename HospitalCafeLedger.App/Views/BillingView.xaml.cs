using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public class OrderItem
{
    public string ItemName { get; set; } = "";
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Qty * Price;
    public override string ToString() => $"{ItemName}  x{Qty}  Rs.{Price}  =  Rs.{Total}";
}

public partial class BillingView : UserControl
{
    private List<OrderItem> _orderItems = new();
    private decimal _total = 0;

    public BillingView()
    {
        InitializeComponent();
    }

    private void DoctorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DoctorListBox.SelectedItem is ListBoxItem item)
            SelectedDoctorLabel.Text = item.Content?.ToString();
    }

    private void QuickItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            var parts = tag.Split('|');
            if (parts.Length == 2 && decimal.TryParse(parts[1], out var price))
                AddItem(parts[0], price);
        }
    }

    private void AddItem(string name, decimal price)
    {
        var existing = _orderItems.FirstOrDefault(x => x.ItemName == name);
        if (existing != null) existing.Qty++;
        else _orderItems.Add(new OrderItem { ItemName = name, Qty = 1, Price = price });
        RefreshList();
    }

    private void RefreshList()
    {
        OrderItemsList.Items.Clear();
        foreach (var item in _orderItems)
            OrderItemsList.Items.Add(item.ToString());
        _total = _orderItems.Sum(x => x.Total);
        TotalAmountLabel.Text = $"Rs. {_total:N0}";
    }

    private void ClearOrderButton_Click(object sender, RoutedEventArgs e)
    {
        _orderItems.Clear();
        RefreshList();
    }

    private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (_orderItems.Count == 0)
        {
            MessageBox.Show("Please add items to the order first.", "No Items", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        MessageBox.Show($"Order saved successfully!\nTotal: Rs. {_total:N0}", "Order Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        _orderItems.Clear();
        RefreshList();
    }

    private void CustomItemButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CustomItemDialog();
        dlg.Owner = Window.GetWindow(this);
        if (dlg.ShowDialog() == true)
            AddItem(dlg.ItemName, dlg.ItemPrice);
    }
}
