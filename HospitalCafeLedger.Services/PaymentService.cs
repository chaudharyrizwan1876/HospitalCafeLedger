using HospitalCafeLedger.Data;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Services;

public class DoctorBalanceSummary
{
    public decimal OpeningBalance    { get; set; }  // Initial deposit
    public decimal TotalDeposits     { get; set; }  // Opening + all cash payments
    public decimal TotalOrders       { get; set; }  // Total orders consumed
    public decimal AvailableBalance  { get; set; }  // TotalDeposits - TotalOrders (can be negative)
    public bool    IsInDebt          { get; set; }  // true if orders > deposits
}

public class PaymentService
{
    public decimal GetTotalOrders(int doctorId)
    {
        using var db = new AppDbContext();
        return db.Orders
            .Where(o => o.DoctorId == doctorId)
            .Sum(o => (decimal?)(o.Quantity * o.Price)) ?? 0m;
    }

    public decimal GetTotalCashPaid(int doctorId)
    {
        using var db = new AppDbContext();
        return db.Payments
            .Where(p => p.DoctorId == doctorId)
            .Sum(p => (decimal?)p.Amount) ?? 0m;
    }

    public List<Payment> GetHistory(int doctorId)
    {
        using var db = new AppDbContext();
        return db.Payments
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.PaymentDate)
            .ToList();
    }

    public void AddPayment(Payment payment)
    {
        using var db = new AppDbContext();
        db.Payments.Add(payment);
        db.SaveChanges();
    }

    /// Wallet logic:
    ///   TotalDeposits    = OpeningBalance + all cash payments added later
    ///   TotalOrders      = all orders placed
    ///   AvailableBalance = TotalDeposits - TotalOrders
    ///   Positive = still has balance to spend
    ///   Negative = owes money
    public DoctorBalanceSummary GetSummary(int doctorId, decimal openingBalance)
    {
        var cashPaid     = GetTotalCashPaid(doctorId);
        var totalDeposits = openingBalance + cashPaid;
        var totalOrders  = GetTotalOrders(doctorId);
        var available    = totalDeposits - totalOrders;

        return new DoctorBalanceSummary
        {
            OpeningBalance   = openingBalance,
            TotalDeposits    = totalDeposits,
            TotalOrders      = totalOrders,
            AvailableBalance = available,
            IsInDebt         = available < 0
        };
    }
}
