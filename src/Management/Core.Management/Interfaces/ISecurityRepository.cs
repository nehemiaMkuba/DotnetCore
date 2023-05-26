using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Core.Domain.Enums;
using Core.Domain.Entities;

namespace Core.Management.Interfaces
{
    public interface ISecurityRepository
    {
        Task<Client> CreateClient(string name, string contactEmail, string description);
        Task<Client> AssignClientRole(Guid clientId, Roles role);
        (string token, long expires) CreateAccessToken(Client client);
        Task<(string token, long expires)> ExtendAccessTokenLifetime(string accessToken, string appSecret);
        Task<Client> AuthenticateClient(Guid apiKey, string appSecret);
        Task<Client> GetClientById(Guid clientId);
        Task<List<Client>> GetClients();
        Task<Client> GetClientFromToken(string accessToken);
        bool ValidateServerKey(string apiKey);
    }
}