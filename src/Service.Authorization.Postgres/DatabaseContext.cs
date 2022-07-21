using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Postgres;
using MyJetWallet.Sdk.Service;
using Service.Authorization.Domain.Models;
using Service.Authorization.Postgres.Models;

namespace Service.Authorization.Postgres
{
public class DatabaseContext : MyDbContext
    {
        public const string Schema = "authorization";

        private const string CredentialsTableName = "authcredentials";
        private const string LogsTableName = "authlogs";
        private const string PinRecordTable = "pinrecord";

        private Activity _activity;

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AuthenticationCredentialsEntity> CredentialsEntities { get; set; }
        
        public DbSet<AuthLogModelDbModel> AuthLogModelDbModels { get; set; }
        
        public DbSet<PinRecord> PinRecords { get; set; }

        public static DatabaseContext Create(DbContextOptionsBuilder<DatabaseContext> options)
        {
            var activity = MyTelemetry.StartActivity($"Database context {Schema}")?.AddTag("db-schema", Schema);

            var ctx = new DatabaseContext(options.Options) {_activity = activity};

            return ctx;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            SetCredentialEntry(modelBuilder);
            SetLogsEntry(modelBuilder);
            SetPinRecordEntity(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SetCredentialEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthenticationCredentialsEntity>().ToTable(CredentialsTableName);
            modelBuilder.Entity<AuthenticationCredentialsEntity>().Property(e => e.Id).HasMaxLength(256);
            modelBuilder.Entity<AuthenticationCredentialsEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<AuthenticationCredentialsEntity>().Property(e => e.Email).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<AuthenticationCredentialsEntity>().Property(e => e.Hash).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<AuthenticationCredentialsEntity>().Property(e => e.Salt).HasMaxLength(256).IsRequired();;
            modelBuilder.Entity<AuthenticationCredentialsEntity>().Property(e => e.Brand).HasMaxLength(128).IsRequired(false);;
            modelBuilder.Entity<AuthenticationCredentialsEntity>().HasIndex(e => e.Id).IsUnique();
        }
        
        private void SetLogsEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthLogModelDbModel>().ToTable(LogsTableName);
            modelBuilder.Entity<AuthLogModelDbModel>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<AuthLogModelDbModel>().HasKey(e => e.Id);
            modelBuilder.Entity<AuthLogModelDbModel>().Property(e => e.TraderId).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<AuthLogModelDbModel>().Property(e => e.Ip).HasMaxLength(128).IsRequired(false);
            modelBuilder.Entity<AuthLogModelDbModel>().Property(e => e.UserAgent).HasMaxLength(256).IsRequired(false);
            modelBuilder.Entity<AuthLogModelDbModel>().Property(e => e.DateTime).HasMaxLength(256).IsRequired();;
            modelBuilder.Entity<AuthLogModelDbModel>().Property(e => e.Location).HasMaxLength(256).IsRequired(false);;
            modelBuilder.Entity<AuthLogModelDbModel>().HasIndex(e => e.TraderId).IsUnique(false);
        }
        
        private void SetPinRecordEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PinRecord>().ToTable(PinRecordTable);
            modelBuilder.Entity<PinRecord>().HasKey(e => e.ClientId);
            modelBuilder.Entity<PinRecord>().Property(e => e.Salt).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<PinRecord>().Property(e => e.Hash).HasMaxLength(256).IsRequired();
        }
        
    }
}