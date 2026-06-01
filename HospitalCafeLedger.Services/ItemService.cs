using HospitalCafeLedger.Data;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Services;

public class ItemService
{
    public List<Item> GetAll()
    {
        using var db = new AppDbContext();
        return db.Items.ToList();
    }

    public void Add(Item item)
    {
        using var db = new AppDbContext();
        db.Items.Add(item);
        db.SaveChanges();
    }

    public void Update(Item item)
    {
        using var db = new AppDbContext();
        var existing = db.Items.Find(item.Id);
        if (existing == null) return;
        existing.Name     = item.Name;
        existing.Category = item.Category;
        existing.Price    = item.Price;
        existing.IsActive = item.IsActive;
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var item = db.Items.Find(id);
        if (item != null)
        {
            db.Items.Remove(item);
            db.SaveChanges();
        }
    }
}
