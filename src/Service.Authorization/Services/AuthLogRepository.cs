using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.Authorization.Postgres;
using Service.Authorization.Postgres.Models;

namespace Service.Authorization.Services
{
    public class AuthLogRepository
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;
        
        public AuthLogRepository(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task Add(IEnumerable<IAuthLogModel> src)
        {
            var modelsToInsert = src.Select(AuthLogModelDbModel.Create);
            await using var ctx = DatabaseContext.Create(_dbContextOptionsBuilder);

            await ctx.AuthLogModelDbModels.AddRangeAsync(modelsToInsert);
            await ctx.SaveChangesAsync();
        }
    }
}