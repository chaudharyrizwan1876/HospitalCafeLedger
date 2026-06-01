namespace HospitalCafeLedger.Models;

public class Payment
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    public string Notes { get; set; } = string.Empty;
}