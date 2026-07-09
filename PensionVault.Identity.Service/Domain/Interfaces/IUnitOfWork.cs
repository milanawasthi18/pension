using System.Threading;
using System.Threading.Tasks;

namespace PensionVault.Identity.Service.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
