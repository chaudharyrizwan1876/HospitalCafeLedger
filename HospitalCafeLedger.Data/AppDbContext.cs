using Microsoft.EntityFrameworkCore;
using HospitalCafeLedger.Models;

namespace HospitalCafeLedger.Data;

public class AppDbContext : DbContext
{
    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=hospitalcafe.db");
    }
}