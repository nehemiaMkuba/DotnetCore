using Core.Domain.Enums;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class IntegrationEvent : AuditableEntity
    {
        public long IntegrationEventId { get; set; }
        public IntegrationStates StatusId { get; set; }//Ready to publish,published
        public string InternalId { get; set; }
        public string QueueId { get; set; }      
        public string EventType { get; set; }
        public string EventBody { get; set; }
        public long BucketId { get; set; } = default;
        public string Response { get; set; }
        public string ThirdPartyId { get; set; }
    }
}