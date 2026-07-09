using Microsoft.EntityFrameworkCore;
using PensionVault.EmployerMember.Service.Domain.Models;

namespace PensionVault.EmployerMember.Service.Infrastructure.Data;

public class EmployerMemberDbContext : DbContext
{
    public EmployerMemberDbContext(DbContextOptions<EmployerMemberDbContext> options) : base(options) { }

    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<FundScheme> FundSchemes => Set<FundScheme>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<FundAccount> FundAccounts => Set<FundAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FundScheme>(e =>
        {
            e.HasKey(x => x.SchemeId);
            e.Property(x => x.SchemeName).HasMaxLength(150).IsRequired();
            e.Property(x => x.SchemeType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EmployeeContributionRate).HasPrecision(5, 2);
            e.Property(x => x.EmployerContributionRate).HasPrecision(5, 2);
            e.Property(x => x.InterestRatePA).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Employer>(e =>
        {
            e.HasKey(x => x.EmployerId);
            e.HasIndex(x => x.RegistrationNumber).IsUnique();
            e.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(x => x.RegistrationNumber).HasMaxLength(100);
            e.Property(x => x.Industry).HasMaxLength(100);
            e.Property(x => x.RemittanceFrequency).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ContactDetails).HasMaxLength(1000);
        });

        modelBuilder.Entity<Member>(e =>
        {
            e.HasKey(x => x.MemberId);
            e.HasIndex(x => x.MembershipNumber).IsUnique();
            e.Property(x => x.MembershipNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Gender).HasMaxLength(10);
            e.Property(x => x.NationalIdRef).HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Employer).WithMany(emp => emp.Members)
                .HasForeignKey(x => x.EmployerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FundAccount>(e =>
        {
            e.HasKey(x => x.AccountId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EmployeeContributionBalance).HasPrecision(18, 2);
            e.Property(x => x.EmployerContributionBalance).HasPrecision(18, 2);
            e.Property(x => x.InterestAccrued).HasPrecision(18, 2);
            e.Property(x => x.TotalBalance).HasPrecision(18, 2);
            e.Property(x => x.VestingPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Member).WithMany(m => m.FundAccounts)
                .HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Scheme).WithMany(s => s.FundAccounts)
                .HasForeignKey(x => x.SchemeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
