using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HospitalCafeLedger.Models;
using HospitalCafeLedger.Services;
 
namespace HospitalCafeLedger.App.Views;
 
// ── ViewModels ────────────────────────────────────────────────
public class OrderLineItem
{
    public string  ItemName     { get; set; } = "";
    public int     Qty          { get; set; }
    public decimal Price        { get; set; }
    public decimal Total        => Qty * Price;
    public string  PriceDisplay => $"Rs. {Price:N0}";
    public string  TotalDisplay => $"Rs. {Total:N0}";
}
 
public class QuickItemVM
{
    public string  Name      { get; set; } = "";
    public decimal ItemPrice { get; set; }
    public string  PriceText => $"Rs. {ItemPrice:N0}";
    public string  Tag       => $"{Name}|{ItemPrice}";
}
 
// Proper class instead of anonymous type — fixes dynamic/reflection errors
public class DoctorListItem
{
    public int    Id      { get; set; }
    public string Display { get; set; } = "";
}
 
// ── View ──────────────────────────────────────────────────────
public partial class BillingView : UserControl
{
    private readonly DoctorService _doctorService = new();
    private readonly ItemService   _itemService   = new();
    private readonly OrderService  _orderService  = new();
 
    private List<Doctor>   _allDoctors = new();
    private Doctor?        _selectedDoctor;
    private readonly ObservableCollection<OrderLineItem> _orderItems = new();
 
    public BillingView()
    {
        InitializeComponent();
        Loaded += (s, e) => Initialize();
    }
 
    private void Initialize()
    {
        OrderItemsList.ItemsSource = _orderItems;
        _orderItems.CollectionChanged += (s, e) => RefreshTotal();
        LoadDoctors();
        LoadQuickItems();
        UpdateCartVisibility();
    }
 
    // ── Doctors ───────────────────────────────────────────────
    private void LoadDoctors()
    {
        _allDoctors = _doctorService.GetAll().Where(d => d.IsActive).ToList();
        BindDoctors(_allDoctors);
    }
 
    private void BindDoctors(List<Doctor> list)
    {
        DoctorListBox.ItemsSource = list
            .Select(d => new DoctorListItem
            {
                Id      = d.Id,
                Display = $"D{d.Id:D3}  {d.Name}"
            })
            .ToList();
        DoctorListBox.DisplayMemberPath = "Display";
    }
 
    private void DoctorSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = DoctorSearchBox.Text.Trim();
 
        // Placeholder visibility
        if (DoctorSearchPlaceholder != null)
            DoctorSearchPlaceholder.Visibility =
                string.IsNullOrEmpty(q) ? Visibility.Visible : Visibility.Collapsed;
 
        var filtered = string.IsNullOrEmpty(q)
            ? _allDoctors
            : _allDoctors.Where(d =>
                d.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.Id.ToString().Contains(q)).ToList();
 
        BindDoctors(filtered);
    }
 
    private void DoctorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DoctorListBox.SelectedItem is not DoctorListItem selected) return;
 
        _selectedDoctor = _allDoctors.FirstOrDefault(d => d.Id == selected.Id);
        if (_selectedDoctor == null) return;
 
        SelectedDoctorLabel.Text  = $"{_selectedDoctor.Name}  (D{_selectedDoctor.Id:D3})";
        OpeningBalanceLabel.Text  = $"Rs. {_selectedDoctor.OpeningBalance:N0}";
    }
 
    // ── Quick Items ───────────────────────────────────────────
    private void LoadQuickItems()
    {
        var items = _itemService.GetAll().Where(i => i.IsActive).ToList();
        QuickItemsPanel.ItemsSource = items
            .Select(i => new QuickItemVM { Name = i.Name, ItemPrice = i.Price })
            .ToList();
    }
 
    private void QuickItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not string tag) return;
 
        var parts = tag.Split('|');
        if (parts.Length == 2 && decimal.TryParse(parts[1], out var price))
            AddToOrder(parts[0], price);
    }
 
    private void CustomItemButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CustomItemDialog { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            AddToOrder(dlg.ItemName, dlg.ItemPrice);
    }
 
    // ── Order ─────────────────────────────────────────────────
    private void AddToOrder(string name, decimal price)
    {
        var existing = _orderItems.FirstOrDefault(x => x.ItemName == name && x.Price == price);
        if (existing != null)
        {
            var idx = _orderItems.IndexOf(existing);
            existing.Qty++;
            _orderItems.RemoveAt(idx);
            _orderItems.Insert(idx, existing);
        }
        else
        {
            _orderItems.Add(new OrderLineItem { ItemName = name, Qty = 1, Price = price });
        }
        UpdateCartVisibility();
    }
 
    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not string name) return;
 
        var item = _orderItems.FirstOrDefault(x => x.ItemName == name);
        if (item != null) _orderItems.Remove(item);
        UpdateCartVisibility();
    }
 
    private void RefreshTotal()
    {
        if (TotalAmountLabel == null) return;
        TotalAmountLabel.Text = $"Rs. {_orderItems.Sum(x => x.Total):N0}";
    }
 
    private void UpdateCartVisibility()
    {
        if (EmptyCartMsg == null) return;
        EmptyCartMsg.Visibility = _orderItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        RefreshTotal();
    }
 
    private void ClearOrderButton_Click(object sender, RoutedEventArgs e)
    {
        _orderItems.Clear();
        UpdateCartVisibility();
    }
 
    private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDoctor == null)
        {
            MessageBox.Show("Please select a doctor first.", "No Doctor Selected",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_orderItems.Count == 0)
        {
            MessageBox.Show("Please add at least one item to the order.", "Empty Order",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
 
        var lines = _orderItems.Select(x => (x.ItemName, x.Qty, x.Price)).ToList();
        _orderService.SaveOrder(_selectedDoctor.Id, lines);
 
        var total = _orderItems.Sum(x => x.Total);
        MessageBox.Show(
            $"Order saved successfully!\n\nDoctor: {_selectedDoctor.Name}\nTotal: Rs. {total:N0}",
            "Order Saved", MessageBoxButton.OK, MessageBoxImage.Information);
 
        _orderItems.Clear();
        UpdateCartVisibility();
    }
}
 