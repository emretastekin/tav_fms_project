using FMS.ReferenceManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace FMS.ReferenceManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // MySQL'de oluşacak tablolarımız
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Station> Stations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // İş Kuralları (Constraints): Havacılıkta iki aynı uçuş veya havalimanı kodu olamaz.
            // Bu yüzden Code alanlarını veritabanı seviyesinde Benzersiz (Unique) yapıyoruz.
            modelBuilder.Entity<Airline>().HasIndex(a => a.Code).IsUnique();
            modelBuilder.Entity<Station>().HasIndex(s => s.Code).IsUnique();
        }
    }
}