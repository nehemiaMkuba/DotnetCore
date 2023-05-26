using Core.Management.Interfaces;
using Core.Domain.Infrastructure.Database;

namespace Core.Management.Repositories
{
    public class DataServiceFactory<T> : IDataServiceFactory<T> where T : class
    {
        public DataServiceFactory(IPNContext context)
        {
            Invoke = new GenericRepository<T>(context);
        }
        public IGenericRepository<T> Invoke { get; }
    }
}
