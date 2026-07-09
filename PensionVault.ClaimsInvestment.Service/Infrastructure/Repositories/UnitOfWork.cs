using PensionVault.ClaimsInvestment.Service.Domain.Interfaces;
using PensionVault.ClaimsInvestment.Service.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PensionVault.ClaimsInvestment.Service.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ClaimsInvestmentDbContext _context;
    public UnitOfWork(ClaimsInvestmentDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
