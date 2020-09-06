using System.Threading.Tasks;
using Grpc.Core;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultApi.ApiContract.TransactionApprovalRequests;
using VaultApi.Common.Persistence.KeyKeepers;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;

namespace VaultApi.GrpcServices
{
    public class TransactionApprovalRequestsService : TransactionApprovalRequests.TransactionApprovalRequestsBase
    {
        private readonly IVaultAgentClient _vaultAgentClient;
        private readonly IKeyKeepersRepository _keyKeepersRepository;

        public TransactionApprovalRequestsService(IVaultAgentClient vaultAgentClient,
            IKeyKeepersRepository keyKeepersRepository)
        {
            _vaultAgentClient = vaultAgentClient;
            _keyKeepersRepository = keyKeepersRepository;
        }

        public override async Task<CreateTransactionApprovalRequestResponse> Create(
            CreateTransactionApprovalRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            if (vaultType != VaultType.Private)
            {
                return GetErrorResponse(
                    CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault only allowed");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue)
            {
                return GetErrorResponse(
                    CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var apiKeyId = context.GetApiKeyId();

            if (!apiKeyId.HasValue)
            {
                return GetErrorResponse(
                    CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "API key id required");
            }

            var tenantId = context.GetTenantId();

            var createRequest = new Swisschain.Sirius.VaultAgent.ApiContract.TransactionApprovalRequests.
                CreateTransactionApprovalRequestRequest
                {
                    RequestId = StringUtils.FormatRequestId(tenantId, request.RequestId),
                    TenantId = tenantId,
                    VaultId = vaultId.Value,
                    ApiKeyId = apiKeyId.Value,
                    TransactionSigningRequestId = request.TransactionSigningRequestId,
                    BlockchainId = request.BlockchainId
                };

            foreach (var keyKeeperRequest in request.KeyKeeperRequests)
            {
                var keyKeeper = await _keyKeepersRepository.GetByKeyIdOrDefaultAsync(keyKeeperRequest.KeyId);

                if (keyKeeper == null)
                {
                    return GetErrorResponse(
                        CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                        $"Key keeper not found. Key id: {keyKeeperRequest.KeyId}");
                }

                createRequest.KeyKeeperRequests.Add(
                    new Swisschain.Sirius.VaultAgent.ApiContract.TransactionApprovalRequests.KeyKeeperRequest
                    {
                        KeyKeeperId = keyKeeper.Id,
                        Message = keyKeeperRequest.Message,
                        Secret = keyKeeperRequest.Secret
                    });
            }

            var createResponse = await _vaultAgentClient.TransactionApprovalRequests.CreateAsync(createRequest);

            if (createResponse.BodyCase == Swisschain.Sirius.VaultAgent.ApiContract.TransactionApprovalRequests
                    .CreateTransactionApprovalRequestResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    createResponse.Error.ErrorMessage);
            }

            return new CreateTransactionApprovalRequestResponse
            {
                Response = new CreateTransactionApprovalRequestResponseBody
                {
                    Id = createResponse.Response.Id,
                    CreatedAt = createResponse.Response.CreatedAt
                }
            };
        }

        private static CreateTransactionApprovalRequestResponse GetErrorResponse(
            CreateTransactionApprovalRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new CreateTransactionApprovalRequestResponse
            {
                Error = new CreateTransactionApprovalRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }
}
