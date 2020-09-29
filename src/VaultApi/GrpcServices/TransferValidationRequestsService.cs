using System.Threading.Tasks;
using Grpc.Core;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultApi.ApiContract.TransferSigninRequests;
using Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests;
using VaultApi.Common.Persistence.TransferValidationRequests;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;

namespace VaultApi.GrpcServices
{
    public class TransferValidationRequestsService : TransferValidationRequests.TransferValidationRequestsBase
    {
        private readonly ITransferValidationRequestRepository _transferValidationRequestRepository;
        private readonly IVaultsRepository _vaultsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;

        public TransferValidationRequestsService(
            ITransferValidationRequestRepository transferValidationRequestRepository, 
            IVaultsRepository vaultsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            _transferValidationRequestRepository = transferValidationRequestRepository;
            _vaultsRepository = vaultsRepository;
            _vaultAgentClient = vaultAgentClient;
        }

        public override async Task<GetTransferValidationRequestsResponse> Get(
            GetTransferValidationRequestsRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(GetTransferValidationRequestsErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(GetTransferValidationRequestsErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            return new GetTransferValidationRequestsResponse();
        }

        public override async Task<ConfirmTransferValidationRequestResponse> Confirm(
            ConfirmTransferValidationRequestRequest request,
            ServerCallContext context)
        {
            return new ConfirmTransferValidationRequestResponse
            {
                Response = new ConfirmTransferValidationRequestResponseBody()
            };
        }

        public override async Task<RejectTransferValidationRequestResponse> Reject(
            RejectTransferValidationRequestRequest request,
            ServerCallContext context)
        {
            return new RejectTransferValidationRequestResponse
            {
                Response = new RejectTransferValidationRequestResponseBody()
            };
        }

        private static GetTransferValidationRequestsResponse GetErrorResponse(
            GetTransferValidationRequestsErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new GetTransferValidationRequestsResponse
            {
                Error = new GetTransferValidationRequestsErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static ConfirmTransferValidationRequestResponse GetErrorResponse(
            ConfirmTransferValidationRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new ConfirmTransferValidationRequestResponse
            {
                Error = new ConfirmTransferValidationRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static RejectTransferValidationRequestResponse GetErrorResponse(
            RejectTransferValidationRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new RejectTransferValidationRequestResponse
            {
                Error = new RejectTransferValidationRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }
}
