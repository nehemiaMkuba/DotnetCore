using System.Threading.Tasks;

namespace Core.Management.Infrastructure.IntegrationEvents.EventBus
{
    public interface IQueueService
    {
        Task<(bool successful, string messageId)> EnqueueMessage(dynamic payload);
    }
}