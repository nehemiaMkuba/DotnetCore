using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using Dapper;

using Core.Domain.Enums;
using Core.Domain.Entities;
using Core.Management.Interfaces;
using Core.Domain.Infrastructure.Database;

namespace Core.Management.Infrastructure.Seedwork
{
    public class Seed : ISeed
    {
        private readonly ILogger<Seed> logger;
        private readonly IPNContext context;
        private readonly IConnection connection;        
        private readonly ISecurityRepository securityRepository;        

        public Seed(IPNContext context, IConnection connection, ILogger<Seed> logger, ISecurityRepository securityRepository)
        {
            this.logger = logger;
            this.context = context;
            this.connection = connection;
            this.securityRepository = securityRepository;
        }

        public async Task SeedDefaults()
        {
            // Create Defaults
            //if (!context.Clients.Any(x => x.Role == Roles.Root))
            //{
            //    Client client = await securityRepository.CreateClient("IPN", "nehemiah@gmail.com", "IPN Client").ConfigureAwait(false);
            //    client = await securityRepository.AssignClientRole(client.ClientId, Roles.Root).ConfigureAwait(false);
            //}

            //UpdateHiLoSequences();
        }
 
        public void UpdateHiLoSequences()
        {
            using SqlConnection sqlConnection = new SqlConnection(connection.ConnectionString);

            int max = sqlConnection.ExecuteScalar<int?>("SELECT MAX(SettingId) FROM IPN.Settings") ?? 0;
            sqlConnection.Execute($"ALTER SEQUENCE [IPN].[Setting_HiLo] RESTART WITH {max += 1} INCREMENT BY 1");

            max = sqlConnection.ExecuteScalar<int?>("SELECT MAX(NotificationIPNId) FROM IPN.NotificationIPNs") ?? 0;
            sqlConnection.Execute($"ALTER SEQUENCE IPN.NotificationIPN_HiLo RESTART WITH {max += 1} INCREMENT BY 1");

            sqlConnection.Execute("ALTER SEQUENCE IPN.Language_HiLo INCREMENT BY 1");
        }
    }
}
