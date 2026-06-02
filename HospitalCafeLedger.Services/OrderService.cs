using HospitalCafeLedger.Data;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Services;

public class OrderService
{
    public void SaveOrder(int doctorId, List<(string name, int qty, decimal price)> items)
    {
        using var db = new AppDbContext();
        foreach (var (name, qty, price) in items)
        {
            // Try to match with existing item in DB, else Id=0 means custom
            var dbItem = db.Items.FirstOrDefault(i => i.Name == name);
            db.Orders.Add(new Order
            {
                DoctorId  = doctorId,
                ItemId    = dbItem?.Id ?? 0,
                Quantity  = qty,
                Price     = price,
                OrderDate = DateTime.Now
            });
        }
        db.SaveChanges();
    }
}
