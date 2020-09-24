using System.Threading.Tasks;
using Grpc.Core;
using Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests;

namespace VaultApi.GrpcServices
{
    public class TransferValidationRequestsService : TransferValidationRequests.TransferValidationRequestsBase
    {
        public TransferValidationRequestsService()
        {
        }

        public override async Task<GetTransferValidationRequestsResponse> Get(
            GetTransferValidationRequestsRequest request,
            ServerCallContext context)
        {
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
