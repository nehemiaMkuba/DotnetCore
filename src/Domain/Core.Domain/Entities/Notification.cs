using System;

using Core.Domain.Enums;
using Core.Domain.Common;

namespace Core.Domain.Entities
{

    public class Notification : AuditableEntity
    {
        public long NotificationId { get; set; }
        public long Msisdn { get; set; } = default;
        public long TransactionReference { get; set; }
        public string? Email { get; set; }
        public int Priority { get; set; } = default;
        public int NumberOfSends { get; set; } = default;
        public string TextBody { get; set; } = null!;
        public string? QueueId { get; set; }
        public string? ProviderId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Subject { get; set; }
        public string? InCopyRecipients { get; set; }
        public DateTime? QueuedAt { get; set; } = default;
        public DateTime? DeliveredAt { get; set; } = default;
        public long BucketId { get; set; } = default;
        public string? SenderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public NotificationStates NotificationStatus { get; set; }
        public InformationModes InformationMode { get; set; }

    }
}