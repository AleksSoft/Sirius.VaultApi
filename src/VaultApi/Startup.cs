using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VaultApi.Common.Configuration;
using VaultApi.Common.Persistence;
using VaultApi.GrpcServices;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.VaultAgent.ApiClient;
using VaultApi.Common.HostedServices;

namespace VaultApi
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
            AddJwtAuth(Config.Auth.JwtSecret, Config.Auth.Audience);
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            base.ConfigureServicesExt(services);

            services
                .AddHttpClient()
                .AddTransient<IVaultAgentClient>(factory => new VaultAgentClient(Config.VaultAgent.Url))
                .AddPersistence(Config.Db.ConnectionString)
                .AddHostedService<DbSchemaValidationHost>();
        }

        protected override void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.RegisterEndpoints(endpoints);

            endpoints.MapGrpcService<MonitoringService>();
            endpoints.MapGrpcService<TransferSigningRequestsService>();
            endpoints.MapGrpcService<TransferValidationRequestsService>();
            endpoints.MapGrpcService<WalletsService>();
        }
    }
}
