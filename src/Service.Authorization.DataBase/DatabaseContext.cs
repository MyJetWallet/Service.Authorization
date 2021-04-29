using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Service.Authorization.DataBase
{
    public class DatabaseContext : DbContext
    {
        public const string Schema = "authorization";

        public const string SessionsTableName = "sessions";
        public const string KillSessionsTableName = "kills";

        public DbSet<KillSessionAuditEntity> KillSessions { get; set; }

        public DbSet<SessionAuditEntity> Sessions { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public static ILoggerFactory LoggerFactory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (LoggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(LoggerFactory).EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<KillSessionAuditEntity>().ToTable(KillSessionsTableName);
            modelBuilder.Entity<KillSessionAuditEntity>().HasKey(e => e.SessionRootId);
            modelBuilder.Entity<KillSessionAuditEntity>().Property(e => e.SessionRootId).HasMaxLength(128);
            modelBuilder.Entity<KillSessionAuditEntity>().Property(e => e.SessionId).HasMaxLength(128);

            modelBuilder.Entity<SessionAuditEntity>().ToTable(SessionsTableName);
            modelBuilder.Entity<SessionAuditEntity>().HasKey(e => e.SessionId);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.SessionRootId).HasMaxLength(128);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.SessionId).HasMaxLength(128);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.BrokerId).HasMaxLength(128);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.BrandId).HasMaxLength(128);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.ClientId).HasMaxLength(128);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.WalletId).HasMaxLength(128);
            modelBuilder.Entity<SessionAuditEntity>().Property(e => e.UserAgent).HasMaxLength(1024);

            base.OnModelCreating(modelBuilder);
        }

        public async Task<int> UpsetAsync(KillSessionAuditEntity entity)
        {
            var result = await KillSessions.Upsert(entity).On(e => e.SessionRootId).NoUpdate().RunAsync();
            return result;
        }

        public async Task<int> UpsetAsync(SessionAuditEntity entity)
        {
            var result = await Sessions.Upsert(entity).On(e => e.SessionId).NoUpdate().RunAsync();
            return result;
        }
    }
}
