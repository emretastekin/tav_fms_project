using FMS.ArchiveManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace FMS.ArchiveManager.Data
{
    public class ArchiveDbContext : DbContext
    {
        // Constructor, veritabanı ayarlarını (connection string) alır
        public ArchiveDbContext(DbContextOptions<ArchiveDbContext> options) : base(options)
        {
        }

        // Arşivlenen uçuşların tutulacağı tablo
        public DbSet<ArchivedFlight> ArchivedFlights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // İstersen burada tablo isimlerini veya kısıtlamaları (constraint) özelleştirebilirsin
            modelBuilder.Entity<ArchivedFlight>().ToTable("ArchivedFlights");
        }
    }
}