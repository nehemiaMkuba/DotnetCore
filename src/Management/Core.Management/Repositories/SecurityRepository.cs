using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

using Dapper;

using Core.Domain.Enums;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Core.Management.Extensions;
using Core.Management.Interfaces;
using Core.Management.Infrastructure;
using Core.Domain.Infrastructure.Database;


namespace Core.Management.Repositories
{
    public class SecurityRepository : ISecurityRepository
    {        
        private readonly IPNContext context;
        private readonly IConnection connection;
        private readonly IConfiguration configuration;
        private readonly IConfigurationRepository configurationRepository;

        public SecurityRepository(IPNContext context,
            IConnection connection,
            IConfigurationRepository configurationRepository,
            IConfiguration configuration)
        {
            this.context = context;
            this.connection = connection;
            this.configurationRepository = configurationRepository;
            this.configuration = configuration;
        }

        public async Task<Client> CreateClient(string name, string contactEmail, string description)
        {
            HelperRepository.ValidatedParameter("Name", name, out name, throwException: true);

            Client client = new Client
            {
                ClientId = Guid.NewGuid(),
                Name = name.ToTitleCase(),
                Secret = $"{Guid.NewGuid():N}".ToUpper(),
                Role = Roles.User,
                AccessTokenLifetimeInMins = configurationRepository.TokenLifetimeInMins,
                AuthorizationCodeLifetimeInMins = configurationRepository.CodeLifetimeInMins,
                IsActive = false,
                ContactEmail = contactEmail?.ToLower() ?? default,
                Description = description ?? default
            };

            await context.Clients.AddAsync(client).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return client;
        }

        public async Task<Client> AuthenticateClient(Guid apiKey, string appSecret)
        {
            Client client = await context.Clients.FindAsync(apiKey).ConfigureAwait(false);

            if (!(client != null && client.IsActive && apiKey == client.ClientId && appSecret == client.Secret)) return null;

            return client;
        }

        public (string token, long expires) CreateAccessToken(Client client)
        {
            //security key for token validation
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Security:Key"]));

            //credentials for signing token
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            DateTime baseDate = DateTime.UtcNow;

            Roles role = client.Role;
            string subjectId = client.ClientId.ToString();
            DateTime expiryDate = baseDate.AddMinutes(client.AccessTokenLifetimeInMins);
            string hashedJti = GenerateJti($"{Guid.NewGuid()}", configuration["Security:Key"]);

            //add claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, $"{hashedJti}"),
                new Claim(JwtRegisteredClaimNames.Sub, $"{subjectId}"),
                new Claim("cli", $"{client.ClientId}"),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            //create token
            JwtSecurityToken jwtToken = new JwtSecurityToken(
                issuer: configuration["Security:Issuer"],
                audience: configuration["Security:Audience"],
                signingCredentials: signingCredentials,
                expires: expiryDate,
                notBefore: baseDate,
                claims: claims);

            string generatedToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return (generatedToken, expiryDate.ToEpoch());
        }

        public async Task<(string token, long expires)> ExtendAccessTokenLifetime(string accessToken, string appSecret)
        {
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadToken(accessToken) as JwtSecurityToken;

            string jti = jwtToken.Claims.First(claim => claim.Type == "jti").Value;
            string sub = jwtToken.Claims.First(claim => claim.Type == "sub").Value;
            Guid cli = Guid.Parse(jwtToken.Claims.First(claim => claim.Type == "cli").Value);

            _ = Convert.FromBase64String(jti);

            Client client = await context.Clients.FindAsync(cli).ConfigureAwait(false);

            if (client is null) throw new Exception($"Invalid cli {cli}");
            if (client.Secret != appSecret) throw new Exception($"Invalid appSecret {appSecret}");

            return CreateAccessToken(client);
        }

        public async Task<Client> AssignClientRole(Guid clientId, Roles role)
        {
            Client client = await context.Clients.FindAsync(clientId).ConfigureAwait(false);

            if (client is null) throw new GenericException($"Client with id '{clientId}' could not be found", "AN001", HttpStatusCode.NotFound);
            if (client.Role == Roles.Root && role != Roles.Root) throw new GenericException("Root role cannot be assigned or revoked", "AN008", HttpStatusCode.Forbidden);

            client.Role = role;
            client.IsActive = true;

            await context.SaveChangesAsync().ConfigureAwait(false);
            return client;
        }

        public async Task<Client> GetClientById(Guid clientId) => await context.Clients.FindAsync(clientId).ConfigureAwait(false);

        public async Task<List<Client>> GetClients() => await context.Clients.ToListAsync().ConfigureAwait(false);

        public async Task<Client> GetClientFromToken(string accessToken)
        {
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadToken(accessToken) as JwtSecurityToken;
            Guid cli = Guid.Parse(jwtToken.Claims.First(claim => claim.Type == "cli").Value);

            string vSQL = Queries.GET_ENTITY_BY_COLUMN_NAME.Replace("{EntityName}", nameof(context.Clients)).Replace("{ColumnName}", nameof(Client.ClientId));
            using SqlConnection sqlConnection = new SqlConnection(connection.ConnectionString);
            Client client = await sqlConnection.QueryFirstOrDefaultAsync<Client>(vSQL, new { value = cli }).ConfigureAwait(false);

            if (client is null) throw new Exception($"Invalid client {cli}");

            return client;
        }

        public static string GenerateJti(string jti, string key)
        {
            ASCIIEncoding asciiEncoding = new ASCIIEncoding();
            byte[] keyBytes = asciiEncoding.GetBytes(key);
            byte[] passwordBytes = asciiEncoding.GetBytes(jti);
            using HMACSHA256 hmacshA256 = new HMACSHA256(keyBytes);
            return Convert.ToBase64String(hmacshA256.ComputeHash(passwordBytes));
        }

        public bool ValidateServerKey(string apiKey) => apiKey == configuration["Events:ConsumerKey"];      

    }
}
