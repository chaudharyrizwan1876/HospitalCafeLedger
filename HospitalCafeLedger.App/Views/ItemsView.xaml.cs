using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HospitalCafeLedger.Models;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

public partial class ItemsView : UserControl
{
    private readonly ItemService _service = new();
    private List<ItemViewModel> _allItems = new();

    public ItemsView()
    {
        InitializeComponent();
        LoadItems();
    }

    private void LoadItems()
    {
        _allItems = _service.GetAll().Select(i => new ItemViewModel
        {
            Id       = i.Id,
            Name     = i.Name,
            Category = i.Category,
            Price    = i.Price,
            IsActive = i.IsActive
        }).ToList();
        RefreshTable(_allItems);
    }

    private void RefreshTable(List<ItemViewModel> items)
    {
        ItemsTable.ItemsSource = null;
        ItemsTable.ItemsSource = items;
        EmptyState.Visibility  = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TotalCountText.Text    = $"Total: {_allItems.Count} items  |  Active: {_allItems.Count(i => i.IsActive)}";
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AddItemDialog { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
        {
            _service.Add(new Item
            {
                Name     = dlg.ItemName,
                Category = dlg.ItemCategory,
                Price    = dlg.ItemPrice,
                IsActive = true
            });
            LoadItems();
            MessageBox.Show($"'{dlg.ItemName}' added successfully!", "Item Added",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;
        var vm = _allItems.FirstOrDefault(i => i.Id == id);
        if (vm == null) return;

        var dlg = new EditItemDialog(vm) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
        {
            _service.Update(new Item
            {
                Id       = vm.Id,
                Name     = dlg.ItemName,
                Category = dlg.ItemCategory,
                Price    = dlg.ItemPrice,
                IsActive = dlg.ItemIsActive
            });
            LoadItems();
            MessageBox.Show($"'{dlg.ItemName}' updated successfully!", "Item Updated",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;
        var vm = _allItems.FirstOrDefault(i => i.Id == id);
        if (vm == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete '{vm.Name}'?\nThis action cannot be undone.",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            _service.Delete(id);
            LoadItems();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = ItemSearchBox.Text.Trim();
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(query) ? Visibility.Visible : Visibility.Collapsed;
        ClearBtn.Visibility          = string.IsNullOrEmpty(query) ? Visibility.Collapsed : Visibility.Visible;

        var filtered = string.IsNullOrEmpty(query)
            ? _allItems
            : _allItems.Where(i =>
                i.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                i.Id.ToString().Contains(query) ||
                i.Category.Contains(query, System.StringComparison.OrdinalIgnoreCase)
              ).ToList();

        RefreshTable(filtered);
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        ItemSearchBox.Text           = "";
        SearchPlaceholder.Visibility = Visibility.Visible;
        ClearBtn.Visibility          = Visibility.Collapsed;
        RefreshTable(_allItems);
    }
}
