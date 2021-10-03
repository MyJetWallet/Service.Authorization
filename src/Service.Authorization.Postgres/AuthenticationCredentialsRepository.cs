using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Service.Authorization.Postgres.Models;

namespace Service.Authorization.Postgres
{
    public class AuthenticationCredentialsRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        private readonly ILogger<AuthenticationCredentialsRepository> _logger;

        private readonly byte[] _initKey;
        private readonly byte[] _initVector;
        
        public AuthenticationCredentialsRepository(ILogger<AuthenticationCredentialsRepository> logger, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, byte[] initKey, byte[] initVector)
        {
            _logger = logger;
            _initKey = initKey;
            _initVector = initVector;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }
        
        private async Task<AuthenticationCredentialsEntity> GetEntityAsync(string email, string brand)
        {
            await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
            var encodeEmail = AuthenticationCredentialsEntity.EncodeEmail(email, _initKey, _initVector);
            return await ctx.CredentialsEntities.FirstOrDefaultAsync(t => t.Email == encodeEmail && t.Brand == brand);
        }
        
        public async Task<IEnumerable<AuthenticationCredentialsEntity>> GetTradersByEmailAsync(string email)
        {
            await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
            var encodeEmail = AuthenticationCredentialsEntity.EncodeEmail(email, _initKey, _initVector);
            return ctx.CredentialsEntities.Where(t => t.Email == encodeEmail);
        }
        
        public async Task<AuthenticationCredentialsEntity> GetByEmailAsync(string email, string brand)
        {
            try
            {
                return await GetEntityAsync(email, brand);
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<AuthenticationCredentialsEntity> GetByIdAsync(string id)
        {
            try
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
                var result = await ctx.CredentialsEntities.FirstOrDefaultAsync(t => t.Id == id.ToLower());

                if (result != null)
                    result.Email = result.Email.Decode(_initKey, _initVector);

                return result;
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<AuthenticationCredentialsEntity> ChangePasswordAsync(string email, string hash, string salt, string brand)
        {
            try
            {
                await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);

                var entity = await GetEntityAsync(email, brand);

                if (entity == null)
                    return null;
            
                entity.SetPassword(hash, salt);

                ctx.CredentialsEntities.Update(entity);
                await ctx.SaveChangesAsync();
                return entity;
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
        
        public async Task AddCredentialsAsync(string email, string hash, string salt, string brand)
        {
            await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);
            var encodeEmail = AuthenticationCredentialsEntity.EncodeEmail(email, _initKey, _initVector);
            var entity = new AuthenticationCredentialsEntity()
            {
                Email = encodeEmail,
                Hash = hash,
                Salt = salt,
                Brand = brand
            };
            await ctx.CredentialsEntities.AddAsync(entity);
            await ctx.SaveChangesAsync();
        }
    }
}