using System;

using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities
{
    public class Client : AuditableEntity
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
        public Roles Role { get; set; }
        public int AccessTokenLifetimeInMins { get; set; } = 60;
        public int AuthorizationCodeLifetimeInMins { get; set; } = 60;
        public bool IsActive { get; set; } = false;
        public string Description { get; set; }
        public string ContactEmail { get; set; }
    }
}