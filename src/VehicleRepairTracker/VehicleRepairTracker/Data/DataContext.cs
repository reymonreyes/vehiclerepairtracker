using Microsoft.EntityFrameworkCore;
using VehicleRepairTracker.Entities;

namespace VehicleRepairTracker.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {            
        }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Repair> Repairs { get; set; }
    }
}
