using PensionVault.Identity.Service.Domain.Interfaces;
using PensionVault.Identity.Service.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PensionVault.Identity.Service.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _context;
    public UnitOfWork(IdentityDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
