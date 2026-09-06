using FMS.FlightManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace FMS.FlightManager.Data
{
    public class FlightDbContext : DbContext
    {
        public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options)
        {
        }

        // PostgreSQL'de oluşacak tablomuz
        public DbSet<Flight> Flights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Uçuş numaraları (Örn: TK1903) sistemde benzersiz olmalıdır
            modelBuilder.Entity<Flight>().HasIndex(f => f.FlightNumber).IsUnique();
        }
    }
}