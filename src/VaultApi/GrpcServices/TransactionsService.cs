using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Transactions;
using Swisschain.Sirius.VaultApi.ApiContract.Transactions;
using VaultApi.Common.Persistence.Transactions;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;

namespace VaultApi.GrpcServices
{
    [Authorize]
    public class TransactionsService : Swisschain.Sirius.VaultApi.ApiContract.Transactions.Transactions.TransactionsBase
    {
        private readonly ITransactionSigningRequestsRepository _transactionSigningRequestsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;

        public TransactionsService(
            ITransactionSigningRequestsRepository transactionSigningRequestsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            _transactionSigningRequestsRepository = transactionSigningRequestsRepository;
            _vaultAgentClient = vaultAgentClient;
        }

        public override async Task<GetTransactionSigningRequestResponse> Get(
            GetTransactionSigningRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(GetTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(GetTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var pendingTransactionSigningRequests = vaultType == VaultType.Shared
                ? await _transactionSigningRequestsRepository.GetPendingForSharedVaultAsync()
                : await _transactionSigningRequestsRepository.GetPendingForPrivateVaultAsync(vaultId.Value);

            var response = new GetTransactionSigningRequestResponseBody();

            response.Requests.AddRange(pendingTransactionSigningRequests
                .Select(transactionSigningRequest =>
                    new Swisschain.Sirius.VaultApi.ApiContract.Transactions.TransactionSigningRequest
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
                                    new Swisschain.Sirius.VaultApi.ApiContract.Transactions.CoinToSpend
                                    {
                                        Id = new Swisschain.Sirius.VaultApi.ApiContract.Transactions.CoinId
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
                                        Value = coinToSpend.Value.ToString(CultureInfo.InvariantCulture),
                                        Redeem = coinToSpend.Redeem,
                                        Address = coinToSpend.Address
                                    })
                        },
                        CreatedAt = Timestamp.FromDateTime(transactionSigningRequest.CreatedAt.UtcDateTime),
                        UpdatedAt = Timestamp.FromDateTime(transactionSigningRequest.UpdatedAt.UtcDateTime)
                    }));

            return new GetTransactionSigningRequestResponse {Response = response};
        }

        public override async Task<ConfirmTransactionSigningRequestResponse> Confirm(
            ConfirmTransactionSigningRequestRequest request,
            ServerCallContext context)
        {
            var tenantId = context.GetTenantId();

            if (string.IsNullOrEmpty(tenantId))
            {
                return GetErrorResponse(
                    ConfirmTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Tenant id required");
            }

            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    ConfirmTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    ConfirmTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var response = await _vaultAgentClient.Transactions.ConfirmAsync(new SignTransactionConfirm
            {
                RequestId = StringUtils.FormatRequestId(tenantId, request.RequestId),
                TenantId = tenantId,
                TransactionSigningRequestId = request.TransactionSigningRequestId,
                SignedTransaction = request.SignedTransaction,
                TransactionId = request.TransactionId
            });

            if (response.BodyCase == SignTransactionConfirmResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    ConfirmTransactionSigningRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    response.Error.ErrorMessage);
            }

            return new ConfirmTransactionSigningRequestResponse
            {
                Response = new ConfirmTransactionSigningRequestResponseBody()
            };
        }

        public override async Task<RejectTransactionSigningRequestResponse> Reject(
            RejectTransactionSigningRequestRequest request,
            ServerCallContext context)
        {
            var tenantId = context.GetTenantId();

            if (string.IsNullOrEmpty(tenantId))
            {
                return GetErrorResponse(
                    RejectTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Tenant id required");
            }

            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    RejectTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    RejectTransactionSigningRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var response = await _vaultAgentClient.Transactions.RejectAsync(new SignTransactionReject
            {
                RequestId = StringUtils.FormatRequestId(tenantId, request.RequestId),
                TenantId = tenantId,
                TransactionSigningRequestId = request.TransactionSigningRequestId,
                Reason = request.Reason switch
                {
                    Swisschain.Sirius.VaultApi.ApiContract.Transactions.TransactionSigningRequestRejectionReason
                        .UnknownBlockchain =>
                    Swisschain.Sirius.VaultAgent.ApiContract.Transactions.TransactionSigningRequestRejectionReason
                        .UnknownBlockchain,
                    Swisschain.Sirius.VaultApi.ApiContract.Transactions.TransactionSigningRequestRejectionReason
                        .UnwantedTransaction =>
                    Swisschain.Sirius.VaultAgent.ApiContract.Transactions.TransactionSigningRequestRejectionReason
                        .UnwantedTransaction,
                    Swisschain.Sirius.VaultApi.ApiContract.Transactions.TransactionSigningRequestRejectionReason
                        .Other =>
                    Swisschain.Sirius.VaultAgent.ApiContract.Transactions.TransactionSigningRequestRejectionReason
                        .Other,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(request.Reason),
                        (int) request.Reason,
                        typeof(Swisschain.Sirius.VaultApi.ApiContract.Transactions.
                            TransactionSigningRequestRejectionReason))
                },
                ReasonMessage = request.ReasonMessage
            });

            if (response.BodyCase == SignTransactionRejectResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    RejectTransactionSigningRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    response.Error.ErrorMessage);
            }

            return new RejectTransactionSigningRequestResponse
            {
                Response = new RejectTransactionSigningRequestResponseBody()
            };
        }

        private static GetTransactionSigningRequestResponse GetErrorResponse(
            GetTransactionSigningRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new GetTransactionSigningRequestResponse
            {
                Error = new GetTransactionSigningRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static ConfirmTransactionSigningRequestResponse GetErrorResponse(
            ConfirmTransactionSigningRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new ConfirmTransactionSigningRequestResponse
            {
                Error = new ConfirmTransactionSigningRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static RejectTransactionSigningRequestResponse GetErrorResponse(
            RejectTransactionSigningRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new RejectTransactionSigningRequestResponse
            {
                Error = new RejectTransactionSigningRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }
}
