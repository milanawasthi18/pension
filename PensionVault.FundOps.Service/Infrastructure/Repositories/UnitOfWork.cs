using PensionVault.FundOps.Service.Domain.Interfaces;
using PensionVault.FundOps.Service.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PensionVault.FundOps.Service.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FundOpsDbContext _context;
    public UnitOfWork(FundOpsDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
