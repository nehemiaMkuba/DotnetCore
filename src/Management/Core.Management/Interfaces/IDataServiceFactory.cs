namespace Core.Management.Interfaces
{
    public interface IDataServiceFactory<T> where T : class
    {
        IGenericRepository<T> Invoke { get; }
    }
}
