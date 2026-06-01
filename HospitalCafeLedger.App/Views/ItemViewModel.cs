namespace HospitalCafeLedger.App.Views;

public class ItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string PriceDisplay => $"Rs. {Price:N0}";
    public string StatusDisplay => IsActive ? "Active" : "Inactive";
}
