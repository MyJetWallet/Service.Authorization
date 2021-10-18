using System;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using MyJetWallet.Sdk.GrpcMetrics;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Postgres;
using MyJetWallet.Sdk.Service;
using Prometheus;
using ProtoBuf.Grpc.Server;
using Service.Authorization.Grpc;
using Service.Authorization.Modules;
using Service.Authorization.Postgres;
using Service.Authorization.Services;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;

namespace Service.Authorization
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.BindCodeFirstGrpc();

            services.AddHostedService<ApplicationLifetimeManager>();
            
            DatabaseContext.LoggerFactory = Program.LogFactory;
            services.AddDatabase(DatabaseContext.Schema, Program.Settings.PostgresConnectionString,
                o => new DatabaseContext(o));
            DatabaseContext.LoggerFactory = null;

            GetEnvVariables();
            
            services.AddMyTelemetry("SP-", Program.Settings.ZipkinUrl);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMetricServer();

            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.BindIsAlive();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcSchema<AuthService, IAuthService>();

                endpoints.MapGrpcSchemaRegistry();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
        }
        
        private void GetEnvVariables()
        {
            var key = Environment.GetEnvironmentVariable(Program.EncodingKeyStr);
            
            if (string.IsNullOrEmpty(key))
                throw new Exception($"Env Variable {Program.EncodingKeyStr} is not found");
            
            var initVector = Environment.GetEnvironmentVariable(Program.EncodingInitVectorStr);
            
            if (string.IsNullOrEmpty(initVector))
                throw new Exception($"Env Variable {Program.EncodingInitVectorStr} is not found");

            Program.EncodingKey = Encoding.UTF8.GetBytes(key);
            Program.EncodingInitVector = Encoding.UTF8.GetBytes(initVector);
        }
    }
}
