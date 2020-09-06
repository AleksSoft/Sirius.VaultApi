using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Swisschain.Sirius.VaultApi.ApiContract.TransactionApprovalConfirmations;
using VaultApi.Common.Persistence.TransactionApprovalConfirmations;

namespace VaultApi.GrpcServices
{
    [Authorize]
    public class TransactionApprovalConfirmationsService
        : TransactionApprovalConfirmations.TransactionApprovalConfirmationsBase
    {
        private readonly ITransactionApprovalConfirmationsRepository _transactionApprovalConfirmationsRepository;

        public TransactionApprovalConfirmationsService(
            ITransactionApprovalConfirmationsRepository transactionApprovalConfirmationsRepository)
        {
            _transactionApprovalConfirmationsRepository = transactionApprovalConfirmationsRepository;
        }

        public override async Task<GetTransactionApprovalConfirmationResponse> Get(
            GetTransactionApprovalConfirmationRequest request,
            ServerCallContext context)
        {
            var transactionApprovalConfirmations = await _transactionApprovalConfirmationsRepository
                .GetByTransactionApprovalRequestIdAsync(request.TransactionApprovalRequestId);

            var response = new GetTransactionApprovalConfirmationResponseBody();

            response.Confirmations.AddRange(transactionApprovalConfirmations
                .Select(confirmation =>
                    new TransactionApprovalConfirmation
                    {
                        Id = confirmation.Id,
                        KeyId = confirmation.KeyId,
                        Message = confirmation.Message,
                        Secret = confirmation.Secret,
                        Status = confirmation.Status switch
                        {
                            Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmationStatus
                                .Confirmed =>
                            TransactionApprovalConfirmation.Types.TransactionApprovalConfirmationStatus.Confirmed,
                            Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmationStatus
                                .Rejected =>
                            TransactionApprovalConfirmation.Types.TransactionApprovalConfirmationStatus.Rejected,
                            Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmationStatus
                                .Skipped =>
                            TransactionApprovalConfirmation.Types.TransactionApprovalConfirmationStatus.Skipped,
                            _ => throw new InvalidEnumArgumentException(nameof(confirmation.Status),
                                (int) confirmation.Status,
                                typeof(Common.ReadModels.TransactionApprovalConfirmations.
                                    TransactionApprovalConfirmationStatus))
                        }
                    }));

            return new GetTransactionApprovalConfirmationResponse {Response = response};
        }
    }
}
