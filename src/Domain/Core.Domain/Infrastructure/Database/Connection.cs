namespace Core.Domain.Infrastructure.Database
{
    public interface IConnection
    {
        string ConnectionString { get; }
    }

    public class Connection : IConnection
    {
        public Connection(string connectionString) => ConnectionString = connectionString;

        public string ConnectionString { get; private set; }
    }
}
