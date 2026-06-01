using HospitalCafeLedger.Data;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Services;

public class DoctorService
{
    // Fresh context har operation ke liye — Detached error fix
    public List<Doctor> GetAll()
    {
        using var db = new AppDbContext();
        return db.Doctors.ToList();
    }

    public void Add(Doctor doctor)
    {
        using var db = new AppDbContext();
        db.Doctors.Add(doctor);
        db.SaveChanges();
    }

    public void Update(Doctor doctor)
    {
        using var db = new AppDbContext();
        // Find existing record, phir values update karo
        var existing = db.Doctors.Find(doctor.Id);
        if (existing == null) return;
        existing.Name          = doctor.Name;
        existing.Department    = doctor.Department;
        existing.PhoneNumber   = doctor.PhoneNumber;
        existing.OpeningBalance = doctor.OpeningBalance;
        existing.IsActive      = doctor.IsActive;
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var doc = db.Doctors.Find(id);
        if (doc != null)
        {
            db.Doctors.Remove(doc);
            db.SaveChanges();
        }
    }
}
