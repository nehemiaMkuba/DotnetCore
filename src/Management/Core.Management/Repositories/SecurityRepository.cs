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
        private readonly IPNContext _context;
        private readonly IConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationRepository _configurationRepository;

        public SecurityRepository(IPNContext context,
            IConnection connection,
            IConfigurationRepository configurationRepository,
            IConfiguration configuration)
        {
            _context = context;
            _connection = connection;
            _configurationRepository = configurationRepository;
            _configuration = configuration;
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
                AccessTokenLifetimeInMins = _configurationRepository.TokenLifetimeInMins,
                AuthorizationCodeLifetimeInMins = _configurationRepository.CodeLifetimeInMins,
                IsActive = false,
                ContactEmail = contactEmail?.ToLower() ?? default,
                Description = description ?? default
            };

            await _context.Clients.AddAsync(client).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return client;
        }

        public async Task<Client> AuthenticateClient(Guid apiKey, string appSecret)
        {
            Client client = await _context.Clients.FindAsync(apiKey).ConfigureAwait(false);

            if (!(client != null && client.IsActive && apiKey == client.ClientId && appSecret == client.Secret)) return null;

            return client;
        }

        public (string token, long expires) CreateAccessToken(Client client)
        {
            //security key for token validation
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Security:Key"]));

            //credentials for signing token
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            DateTime baseDate = DateTime.UtcNow;

            Roles role = client.Role;
            string subjectId = client.ClientId.ToString();
            DateTime expiryDate = baseDate.AddMinutes(client.AccessTokenLifetimeInMins);
            string hashedJti = GenerateJti($"{Guid.NewGuid()}", _configuration["Security:Key"]);

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
                issuer: _configuration["Security:Issuer"],
                audience: _configuration["Security:Audience"],
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

            Client client = await _context.Clients.FindAsync(cli).ConfigureAwait(false);

            if (client is null) throw new Exception($"Invalid cli {cli}");
            if (client.Secret != appSecret) throw new Exception($"Invalid appSecret {appSecret}");

            return CreateAccessToken(client);
        }

        public async Task<Client> AssignClientRole(Guid clientId, Roles role)
        {
            Client client = await _context.Clients.FindAsync(clientId).ConfigureAwait(false);

            if (client is null) throw new GenericException($"Client with id '{clientId}' could not be found", "AN001", HttpStatusCode.NotFound);
            if (client.Role == Roles.Root && role != Roles.Root) throw new GenericException("Root role cannot be assigned or revoked", "AN008", HttpStatusCode.Forbidden);

            client.Role = role;
            client.IsActive = true;

            await _context.SaveChangesAsync().ConfigureAwait(false);
            return client;
        }

        public async Task<Client> GetClientById(Guid clientId) => await _context.Clients.FindAsync(clientId).ConfigureAwait(false);

        public async Task<List<Client>> GetClients() => await _context.Clients.ToListAsync().ConfigureAwait(false);

        public async Task<Client> GetClientFromToken(string accessToken)
        {
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadToken(accessToken) as JwtSecurityToken;
            Guid cli = Guid.Parse(jwtToken.Claims.First(claim => claim.Type == "cli").Value);

            string vSQL = Queries.GET_ENTITY_BY_COLUMN_NAME.Replace("{EntityName}", nameof(_context.Clients)).Replace("{ColumnName}", nameof(Client.ClientId));
            using SqlConnection sqlConnection = new SqlConnection(_connection.ConnectionString);
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

        public bool ValidateServerKey(string apiKey) => apiKey == _configuration["Events:ConsumerKey"];      

    }
}
