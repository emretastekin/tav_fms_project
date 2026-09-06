using Microsoft.EntityFrameworkCore;
using FMS.Security.Entities;

namespace FMS.Security.Data
{
    public class FmsAuthDbContext : DbContext
    {
        public FmsAuthDbContext(DbContextOptions<FmsAuthDbContext> options) : base(options) 
        { 
        }

        // Tablo karşılıklarımız
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. UserRole Çoka Çok (Many-to-Many) İlişkisi
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId }); // Composite Primary Key

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // 2. RolePermission Çoka Çok (Many-to-Many) İlişkisi
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId }); // Composite Primary Key

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
                
            // 3. FirebaseUid için Unique Index (Performans ve Güvenlik İçin)
            // Sistemdeki bir Firebase kullanıcısının birden fazla kaydı olmasını engelliyoruz
            modelBuilder.Entity<User>()
                .HasIndex(u => u.FirebaseUid)
                .IsUnique();
        }
    }
}