using Microsoft.EntityFrameworkCore;
using PensionVault.Identity.Service.Domain.Models;

namespace PensionVault.Identity.Service.Infrastructure.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.AuditId);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.RecordId).HasMaxLength(100);
            e.HasOne(x => x.User).WithMany(u => u.AuditLogs)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
