namespace HospitalCafeLedger.Models;

public class Doctor
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }

    public bool IsActive { get; set; } = true;
}