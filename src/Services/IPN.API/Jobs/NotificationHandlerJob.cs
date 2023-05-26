using Quartz;
using System.Threading.Tasks;

using Core.Management.Infrastructure.IntegrationEvents.EventHandling;


namespace IPN.API.Jobs
{
    [DisallowConcurrentExecution]
    public class NotificationHandlerJob : IJob
    {

        private readonly INotificationHandler _handler;

        public NotificationHandlerJob(INotificationHandler handler)
        {
            _handler = handler;
        }
        public async Task Execute(IJobExecutionContext context)
        {
          await  _handler.Handle();
        }
    }
}
