using System.Threading;
using System.Threading.Tasks;

namespace PensionVault.ClaimsInvestment.Service.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
