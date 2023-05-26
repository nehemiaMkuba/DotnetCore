using System.Threading.Tasks;

namespace Core.Management.Infrastructure.IntegrationEvents.EventHandling
{
   

    public interface INotificationHandler
    {
        Task Handle();
    }
    
}
