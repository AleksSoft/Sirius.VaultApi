using System.Threading.Tasks;
using Grpc.Core;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sirius.VaultApi.ApiContract.Monitoring;

namespace VaultApi.GrpcServices
{
    public class MonitoringService : Monitoring.MonitoringBase
    {
        public override Task<IsAliveResponse> IsAlive(IsAliveRequest request, ServerCallContext context)
        {
            var result = new IsAliveResponse
            {
                Name = ApplicationInformation.AppName,
                Version = ApplicationInformation.AppVersion,
                StartedAt = ApplicationInformation.StartedAt.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return Task.FromResult(result);
        }
    }
}
