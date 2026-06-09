using System.Windows;
using System.Windows.Controls;
using HospitalCafeLedger.Models;
using HospitalCafeLedger.Services;

namespace HospitalCafeLedger.App.Views;

public class DoctorViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; }
    public string OpeningBalanceDisplay => $"Rs. {OpeningBalance:N0}";
    public string StatusDisplay => IsActive ? "Active" : "Inactive";
}

public partial class DoctorsView : UserControl
{
    private readonly DoctorService _service = new();
    private List<DoctorViewModel> _allDoctors = new();

    public DoctorsView()
    {
        InitializeComponent();
        LoadDoctors();
    }

    private void LoadDoctors()
    {
        _allDoctors = _service.GetAll().Select(d => new DoctorViewModel
        {
            Id = d.Id,
            Name = d.Name,
            Department = d.Department,
            PhoneNumber = d.PhoneNumber,
            OpeningBalance = d.OpeningBalance,
            IsActive = d.IsActive
        }).ToList();
        RefreshTable(_allDoctors);
    }

    private void RefreshTable(List<DoctorViewModel> doctors)
    {
        DoctorsTable.ItemsSource = null;
        DoctorsTable.ItemsSource = doctors;
        EmptyState.Visibility = doctors.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TotalCountText.Text = $"Total: {_allDoctors.Count} doctors  |  Active: {_allDoctors.Count(d => d.IsActive)}";
    }

    private void AddDoctor_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AddDoctorDialog { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
        {
            _service.Add(new Doctor
            {
                Name = dlg.DoctorName,
                Department = dlg.Department,
                PhoneNumber = dlg.Phone,
                OpeningBalance = dlg.OpeningBalance,
                IsActive = true
            });
            LoadDoctors();
            MessageBox.Show($"'{dlg.DoctorName}' successfully added!", "Doctor Added",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;
        var vm = _allDoctors.FirstOrDefault(d => d.Id == id);
        if (vm == null) return;

        var dlg = new EditDoctorDialog(vm) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
        {
            _service.Update(new Doctor
            {
                Id = vm.Id,
                Name = dlg.DoctorName,
                Department = dlg.Department,
                PhoneNumber = dlg.Phone,
                OpeningBalance = dlg.OpeningBalance,
                IsActive = dlg.DoctorIsActive
            });
            LoadDoctors();
            MessageBox.Show($"'{dlg.DoctorName}' updated successfully!", "Doctor Updated",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;
        var vm = _allDoctors.FirstOrDefault(d => d.Id == id);
        if (vm == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete '{vm.Name}'?\nThis action cannot be undone.",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            _service.Delete(id);
            LoadDoctors();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = DoctorSearchBox.Text.Trim();
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(query) ? Visibility.Visible : Visibility.Collapsed;
        ClearBtn.Visibility = string.IsNullOrEmpty(query) ? Visibility.Collapsed : Visibility.Visible;

        var filtered = string.IsNullOrEmpty(query)
            ? _allDoctors
            : _allDoctors.Where(d =>
                d.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                d.Id.ToString().Contains(query) ||
                d.Department.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                d.PhoneNumber.Contains(query)).ToList();
        RefreshTable(filtered);
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        DoctorSearchBox.Text = "";
        SearchPlaceholder.Visibility = Visibility.Visible;
        ClearBtn.Visibility = Visibility.Collapsed;
        RefreshTable(_allDoctors);
    }
}
