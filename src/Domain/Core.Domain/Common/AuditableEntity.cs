using System;
using Core.Domain.Infrastructure.Services;

namespace Core.Domain.Common
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToInstanceDate();
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow.ToInstanceDate();
    }
}
