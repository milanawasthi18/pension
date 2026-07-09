using Microsoft.EntityFrameworkCore;
using PensionVault.ClaimsInvestment.Service.Domain.Models;

namespace PensionVault.ClaimsInvestment.Service.Infrastructure.Data;

public class ClaimsInvestmentDbContext : DbContext
{
    public ClaimsInvestmentDbContext(DbContextOptions<ClaimsInvestmentDbContext> options) : base(options) { }

    public DbSet<BenefitClaim> BenefitClaims => Set<BenefitClaim>();
    public DbSet<ClaimDisbursement> ClaimDisbursements => Set<ClaimDisbursement>();
    public DbSet<InvestmentPortfolio> InvestmentPortfolios => Set<InvestmentPortfolio>();
    public DbSet<CorpusRecord> CorpusRecords => Set<CorpusRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BenefitClaim>(e =>
        {
            e.HasKey(x => x.ClaimId);
            e.Property(x => x.ClaimType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            e.Property(x => x.VestedAmount).HasPrecision(18, 2);
            e.Property(x => x.TaxDeductible).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ClaimDisbursement>(e =>
        {
            e.HasKey(x => x.DisbursementId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.DisbursedAmount).HasPrecision(18, 2);
            e.Property(x => x.TaxDeducted).HasPrecision(18, 2);
            e.Property(x => x.NetAmount).HasPrecision(18, 2);
            e.Property(x => x.BankAccountRef).HasMaxLength(100);
            e.HasOne(x => x.Claim).WithMany(c => c.Disbursements)
                .HasForeignKey(x => x.ClaimId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvestmentPortfolio>(e =>
        {
            e.HasKey(x => x.PortfolioId);
            e.Property(x => x.AssetClass).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.AllocationPercent).HasPrecision(5, 2);
            e.Property(x => x.InvestedValue).HasPrecision(18, 2);
            e.Property(x => x.CurrentValue).HasPrecision(18, 2);
            e.Property(x => x.YieldEarned).HasPrecision(5, 2);
        });

        modelBuilder.Entity<CorpusRecord>(e =>
        {
            e.HasKey(x => x.CorpusId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalContributions).HasPrecision(18, 2);
            e.Property(x => x.TotalWithdrawals).HasPrecision(18, 2);
            e.Property(x => x.InvestmentIncome).HasPrecision(18, 2);
            e.Property(x => x.ManagementExpenses).HasPrecision(18, 2);
            e.Property(x => x.ClosingCorpus).HasPrecision(18, 2);
        });
    }
}
