using HospitalCafeLedger.Data;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Services;

public class DoctorReportRow
{
    public int     DoctorId         { get; set; }
    public string  DoctorName       { get; set; } = "";
    public decimal OpeningBalance   { get; set; }
    public decimal TotalDeposits    { get; set; }   // Opening + cash payments
    public decimal TotalOrders      { get; set; }   // Orders in selected period
    public decimal AvailableBalance { get; set; }   // TotalDeposits - AllTimeOrders
    public bool    IsInDebt         => AvailableBalance < 0;
}

public class ReportResult
{
    public int    Month             { get; set; }
    public int    Year              { get; set; }
    public string PeriodLabel       { get; set; } = "";
    public List<DoctorReportRow> Rows { get; set; } = new();

    // Grand totals
    public decimal GrandOpeningBalance   => Rows.Sum(r => r.OpeningBalance);
    public decimal GrandTotalDeposits    => Rows.Sum(r => r.TotalDeposits);
    public decimal GrandTotalOrders      => Rows.Sum(r => r.TotalOrders);
    public decimal GrandAvailableBalance => Rows.Sum(r => r.AvailableBalance);
}

public class ReportService
{
    /// Returns years that have any orders or payments (plus current year)
    public List<int> GetAvailableYears()
    {
        using var db = new AppDbContext();
        var orderYears   = db.Orders.Select(o => o.OrderDate.Year).Distinct().ToList();
        var paymentYears = db.Payments.Select(p => p.PaymentDate.Year).Distinct().ToList();
        var years = orderYears.Union(paymentYears)
                              .Union(new[] { DateTime.Now.Year })
                              .OrderByDescending(y => y)
                              .ToList();
        return years;
    }

    public ReportResult Generate(int month, int year)
    {
        using var db = new AppDbContext();

        var doctors = db.Doctors.Where(d => d.IsActive).ToList();
        var rows    = new List<DoctorReportRow>();

        foreach (var doc in doctors)
        {
            // Orders in selected month/year
            var monthOrders = db.Orders
                .Where(o => o.DoctorId == doc.Id
                         && o.OrderDate.Month == month
                         && o.OrderDate.Year  == year)
                .Sum(o => (decimal?)(o.Quantity * o.Price)) ?? 0m;

            // All-time cash deposits
            var allCashPaid = db.Payments
                .Where(p => p.DoctorId == doc.Id)
                .Sum(p => (decimal?)p.Amount) ?? 0m;

            // All-time orders (for wallet balance)
            var allOrders = db.Orders
                .Where(o => o.DoctorId == doc.Id)
                .Sum(o => (decimal?)(o.Quantity * o.Price)) ?? 0m;

            var totalDeposits = doc.OpeningBalance + allCashPaid;
            var available     = totalDeposits - allOrders;

            rows.Add(new DoctorReportRow
            {
                DoctorId         = doc.Id,
                DoctorName       = doc.Name,
                OpeningBalance   = doc.OpeningBalance,
                TotalDeposits    = totalDeposits,
                TotalOrders      = monthOrders,     // period-specific
                AvailableBalance = available         // all-time wallet balance
            });
        }

        var months = new[]
        {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };

        return new ReportResult
        {
            Month        = month,
            Year         = year,
            PeriodLabel  = $"{months[month - 1]} {year}",
            Rows         = rows
        };
    }
}
