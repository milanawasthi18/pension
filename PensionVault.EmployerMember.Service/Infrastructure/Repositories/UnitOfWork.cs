using PensionVault.EmployerMember.Service.Domain.Interfaces;
using PensionVault.EmployerMember.Service.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EmployerMemberDbContext _context;
    public UnitOfWork(EmployerMemberDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
