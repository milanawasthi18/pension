using Microsoft.EntityFrameworkCore;
using PensionVault.FundOps.Service.Domain.Models;

namespace PensionVault.FundOps.Service.Infrastructure.Data;

public class FundOpsDbContext : DbContext
{
    public FundOpsDbContext(DbContextOptions<FundOpsDbContext> options) : base(options) { }

    public DbSet<ContributionRemittance> ContributionRemittances => Set<ContributionRemittance>();
    public DbSet<MemberContribution> MemberContributions => Set<MemberContribution>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<InterestCreditRecord> InterestCreditRecords => Set<InterestCreditRecord>();
    public DbSet<AnnuityPlan> AnnuityPlans => Set<AnnuityPlan>();
    public DbSet<MonthlyPensionDisbursement> MonthlyPensionDisbursements => Set<MonthlyPensionDisbursement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContributionRemittance>(e =>
        {
            e.HasKey(x => x.RemittanceId);
            e.Property(x => x.RemittancePeriod).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalEmployeeShare).HasPrecision(18, 2);
            e.Property(x => x.TotalEmployerShare).HasPrecision(18, 2);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<MemberContribution>(e =>
        {
            e.HasKey(x => x.ContributionId);
            e.Property(x => x.Period).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EmployeeAmount).HasPrecision(18, 2);
            e.Property(x => x.EmployerAmount).HasPrecision(18, 2);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Remittance).WithMany(r => r.MemberContributions)
                .HasForeignKey(x => x.RemittanceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.EntryId);
            e.Property(x => x.EntryType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            e.Property(x => x.ReferenceId).HasMaxLength(100);
        });

        modelBuilder.Entity<InterestCreditRecord>(e =>
        {
            e.HasKey(x => x.InterestId);
            e.Property(x => x.FinancialYear).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            e.Property(x => x.TotalContributions).HasPrecision(18, 2);
            e.Property(x => x.InterestRateApplied).HasPrecision(5, 2);
            e.Property(x => x.InterestAmount).HasPrecision(18, 2);
            e.Property(x => x.ClosingBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<AnnuityPlan>(e =>
        {
            e.HasKey(x => x.AnnuityId);
            e.Property(x => x.PlanType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PurchaseValue).HasPrecision(18, 2);
            e.Property(x => x.MonthlyPension).HasPrecision(18, 2);
        });

        modelBuilder.Entity<MonthlyPensionDisbursement>(e =>
        {
            e.HasKey(x => x.DisbursementId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.GrossAmount).HasPrecision(18, 2);
            e.Property(x => x.TaxDeducted).HasPrecision(18, 2);
            e.Property(x => x.NetAmount).HasPrecision(18, 2);
            e.HasOne(x => x.AnnuityPlan).WithMany(a => a.PensionDisbursements)
                .HasForeignKey(x => x.AnnuityId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
