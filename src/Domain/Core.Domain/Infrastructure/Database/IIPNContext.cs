using System.Threading;
using System.Threading.Tasks;

namespace Core.Domain.Infrastructure.Database
{
    public interface IIPNContext
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}