using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Transactions;
using Swisschain.Sirius.VaultApi.ApiContract.TransferSigninRequests;
using VaultApi.Common.Persistence.Transactions;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;

namespace VaultApi.GrpcServices
{
    [Authorize]
    public class TransferSigningRequestsService : TransferSigningRequests.TransferSigningRequestsBase
    {
        private readonly ITransactionSigningRequestsRepository _transactionSigningRequestsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;

        public TransferSigningRequestsService(
            ITransactionSigningRequestsRepository transactionSigningRequestsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            _transactionSigningRequestsRepository = transactionSigningRequestsRepository;
            _vaultAgentClient = vaultAgentClient;
        }

        public override async Task<GetTransferSigningRequestsResponse> Get(GetTransferSigningRequestsRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(GetTransferSigningRequestsErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(GetTransferSigningRequestsErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var pendingTransactionSigningRequests = vaultType == VaultType.Shared
                ? await _transactionSigningRequestsRepository.GetPendingForSharedVaultAsync()
                : await _transactionSigningRequestsRepository.GetPendingForPrivateVaultAsync(vaultId.Value);

            var response = new GetTransferSigningRequestsResponseBody();

            response.Requests.AddRange(pendingTransactionSigningRequests
                .Select(transactionSigningRequest =>
                    new TransferSigningRequest
                    {
                        Id = transactionSigningRequest.Id,
                        BlockchainId = transactionSigningRequest.BlockchainId,
                        NetworkType = transactionSigningRequest.NetworkType switch
                        {
                            Swisschain.Sirius.Sdk.Primitives.NetworkType.Private =>
                            Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Private,
                            Swisschain.Sirius.Sdk.Primitives.NetworkType.Public =>
                            Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Public,
                            Swisschain.Sirius.Sdk.Primitives.NetworkType.Test =>
                            Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Test,
                            _ => throw new InvalidEnumArgumentException(nameof(transactionSigningRequest.NetworkType),
                                (int) transactionSigningRequest.NetworkType,
                                typeof(Swisschain.Sirius.Sdk.Primitives.NetworkType))
                        },
                        ProtocolCode = transactionSigningRequest.ProtocolCode,
                        DoubleSpendingProtectionType = transactionSigningRequest.DoubleSpendingProtectionType switch
                        {
                            Swisschain.Sirius.Sdk.Primitives.DoubleSpendingProtectionType.Nonce =>
                            DoubleSpendingProtectionType.Nonce,
                            Swisschain.Sirius.Sdk.Primitives.DoubleSpendingProtectionType.Coins =>
                            DoubleSpendingProtectionType.Coins,
                            _ => throw new InvalidEnumArgumentException(
                                nameof(transactionSigningRequest.DoubleSpendingProtectionType),
                                (int) transactionSigningRequest.DoubleSpendingProtectionType,
                                typeof(Swisschain.Sirius.Sdk.Primitives.DoubleSpendingProtectionType))
                        },
                        BuiltTransaction = ByteString.CopyFrom(transactionSigningRequest.BuiltTransaction.ToArray()),
                        SigningAddresses = {transactionSigningRequest.SigningAddresses},
                        CoinsToSpend =
                        {
                            transactionSigningRequest.CoinsToSpend
                                .Select(coinToSpend =>
                                    new Swisschain.Sirius.VaultApi.ApiContract.TransferSigninRequests.CoinToSpend
                                    {
                                        Id =
                                            new Swisschain.Sirius.VaultApi.ApiContract.TransferSigninRequests.CoinId
                                            {
                                                Number = coinToSpend.Id.Number,
                                                TransactionId = coinToSpend.Id.TransactionId
                                            },
                                        Asset = new BlockchainAsset
                                        {
                                            Id = new BlockchainAssetId
                                            {
                                                Address = coinToSpend.Asset.Id.Address,
                                                Symbol = coinToSpend.Asset.Id.Symbol
                                            },
                                            Accuracy = coinToSpend.Asset.Accuracy
                                        },
                                        Value = coinToSpend.Value,
                                        Redeem = coinToSpend.Redeem,
                                        Address = coinToSpend.Address
                                    })
                        },
                        PolicyResult = "PolicyResult", // TODO:
                        GuardianSignature = "GuardianSignature", // TODO:
                        CreatedAt = Timestamp.FromDateTime(transactionSigningRequest.CreatedAt.UtcDateTime),
                        UpdatedAt = Timestamp.FromDateTime(transactionSigningRequest.UpdatedAt.UtcDateTime),
                        Group = transactionSigningRequest.Group,
                        TenantId = transactionSigningRequest.TenantId
                    }));

            return new GetTransferSigningRequestsResponse {Response = response};
        }

        public override async Task<ConfirmTransferSigningRequestResponse> Confirm(
            ConfirmTransferSigningRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    ConfirmTransferSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    ConfirmTransferSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var transactionSigningRequest = await _transactionSigningRequestsRepository
                .GetByIdAsync(request.TransferSigningRequestId);

            var response = await _vaultAgentClient.Transactions.ConfirmAsync(new SignTransactionConfirm
            {
                RequestId = StringUtils.FormatRequestId(transactionSigningRequest.TenantId, request.RequestId),
                TenantId = transactionSigningRequest.TenantId,
                TransactionSigningRequestId = request.TransferSigningRequestId,
                SignedTransaction = request.SignedTransaction,
                TransactionId = request.TransactionId,
                // TODO: VaultSignature = request.VaultSignature, 
                // TODO: HostProcessId = request.HostProcessId
            });

            if (response.BodyCase == SignTransactionConfirmResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    ConfirmTransferSigningRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    response.Error.ErrorMessage);
            }

            return new ConfirmTransferSigningRequestResponse
            {
                Response = new ConfirmTransferSigningRequestResponseBody()
            };
        }

        public override async Task<RejectTransferSigningRequestResponse> Reject(
            RejectTransferSigningRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    RejectTransferSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    RejectTransferSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var transactionSigningRequest = await _transactionSigningRequestsRepository
                .GetByIdAsync(request.TransferSigningRequestId);

            var response = await _vaultAgentClient.Transactions.RejectAsync(new SignTransactionReject
            {
                RequestId = StringUtils.FormatRequestId(transactionSigningRequest.TenantId, request.RequestId),
                TenantId = transactionSigningRequest.TenantId,
                TransactionSigningRequestId = request.TransferSigningRequestId,
                Reason = request.Reason switch
                {
                    TransferSigningRequestRejectionReason.UnknownBlockchain => TransactionSigningRequestRejectionReason
                        .UnknownBlockchain,
                    TransferSigningRequestRejectionReason.InvalidSignature => TransactionSigningRequestRejectionReason
                        .UnwantedTransaction, // TODO:
                    TransferSigningRequestRejectionReason.Other => TransactionSigningRequestRejectionReason
                        .Other,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(request.Reason),
                        (int) request.Reason,
                        typeof(TransferSigningRequestRejectionReason))
                },
                ReasonMessage = request.ReasonMessage,
                // TODO: VaultSignature = request.VaultSignature, 
                // TODO: HostProcessId = request.HostProcessId
            });

            if (response.BodyCase == SignTransactionRejectResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    RejectTransferSigningRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    response.Error.ErrorMessage);
            }

            return new RejectTransferSigningRequestResponse {Response = new RejectTransferSigningRequestResponseBody()};
        }

        private static GetTransferSigningRequestsResponse GetErrorResponse(
            GetTransferSigningRequestsErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new GetTransferSigningRequestsResponse
            {
                Error = new GetTransferSigningRequestsErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static ConfirmTransferSigningRequestResponse GetErrorResponse(
            ConfirmTransferSigningRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new ConfirmTransferSigningRequestResponse
            {
                Error = new ConfirmTransferSigningRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static RejectTransferSigningRequestResponse GetErrorResponse(
            RejectTransferSigningRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new RejectTransferSigningRequestResponse
            {
                Error = new RejectTransferSigningRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }
}
