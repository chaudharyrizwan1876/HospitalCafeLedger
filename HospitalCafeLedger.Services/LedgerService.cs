using HospitalCafeLedger.Data;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Services;

public class LedgerOrderRow
{
    public DateTime Date      { get; set; }
    public string   ItemName  { get; set; } = "";
    public int      Qty       { get; set; }
    public decimal  Price     { get; set; }
    public decimal  Total     => Qty * Price;
}

public class LedgerDaySummary
{
    public DateTime          Date  { get; set; }
    public List<LedgerOrderRow> Rows { get; set; } = new();
    public decimal           DayTotal => Rows.Sum(r => r.Total);
}

public class LedgerSummary
{
    public decimal OpeningBalance   { get; set; }
    public decimal TotalDeposits    { get; set; }   // Opening + cash payments
    public decimal TotalOrders      { get; set; }
    public decimal AvailableBalance { get; set; }   // TotalDeposits - TotalOrders
    public bool    IsInDebt         { get; set; }
    public List<LedgerDaySummary> DayGroups { get; set; } = new();
}

public class LedgerService
{
    public LedgerSummary GetLedger(int doctorId, decimal openingBalance)
    {
        using var db = new AppDbContext();

        // Get all orders with item names
        var orders = db.Orders
            .Where(o => o.DoctorId == doctorId)
            .OrderBy(o => o.OrderDate)
            .ToList();

        // Get item names lookup
        var itemIds  = orders.Select(o => o.ItemId).Distinct().ToList();
        var items    = db.Items
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionary(i => i.Id, i => i.Name);

        // Group by date
        var dayGroups = orders
            .GroupBy(o => o.OrderDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new LedgerDaySummary
            {
                Date = g.Key,
                Rows = g.Select(o => new LedgerOrderRow
                {
                    Date     = o.OrderDate,
                    ItemName = items.TryGetValue(o.ItemId, out var n) ? n : "Custom Item",
                    Qty      = o.Quantity,
                    Price    = o.Price
                }).ToList()
            }).ToList();

        // Cash deposits
        var cashPaid = db.Payments
            .Where(p => p.DoctorId == doctorId)
            .Sum(p => (decimal?)p.Amount) ?? 0m;

        var totalDeposits = openingBalance + cashPaid;
        var totalOrders   = orders.Sum(o => o.Quantity * o.Price);
        var available     = totalDeposits - totalOrders;

        return new LedgerSummary
        {
            OpeningBalance   = openingBalance,
            TotalDeposits    = totalDeposits,
            TotalOrders      = totalOrders,
            AvailableBalance = available,
            IsInDebt         = available < 0,
            DayGroups        = dayGroups
        };
    }
}
