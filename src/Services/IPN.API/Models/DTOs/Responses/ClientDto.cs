using System;
using Core.Domain.Enums;

namespace IPN.API.Models.DTOs.Responses
{
    public class ClientDto : MinifiedClientDto
    {
        public Roles Role { get; set; }
        public int AccessTokenLifetimeInMins { get; set; }
        public int AuthorizationCodeLifetimeInMins { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

}
