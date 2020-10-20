using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultApi.ApiContract.TransferSigninRequests;
using VaultApi.Common.Persistence.TransferSigningRequests;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;

namespace VaultApi.GrpcServices
{
    [Authorize]
    public class TransferSigningRequestsService : TransferSigningRequests.TransferSigningRequestsBase
    {
        private readonly ITransferSigningRequestsRepository _transferSigningRequestsRepository;
        private readonly IVaultAgentClient _vaultAgentClient;

        public TransferSigningRequestsService(ITransferSigningRequestsRepository transferSigningRequestsRepository,
            IVaultAgentClient vaultAgentClient)
        {
            _transferSigningRequestsRepository = transferSigningRequestsRepository;
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
                ? await _transferSigningRequestsRepository.GetPendingForSharedVaultAsync()
                : await _transferSigningRequestsRepository.GetPendingForPrivateVaultAsync(vaultId.Value);

            var response = new GetTransferSigningRequestsResponseBody();

            foreach (var transactionSigningRequest in pendingTransactionSigningRequests)
            {
                var item = new TransferSigningRequest
                {
                    Id = transactionSigningRequest.Id,
                    BlockchainId = transactionSigningRequest.Blockchain.Id,
                    NetworkType = transactionSigningRequest.Blockchain.NetworkType switch
                    {
                        Swisschain.Sirius.Sdk.Primitives.NetworkType.Private =>
                        Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Private,
                        Swisschain.Sirius.Sdk.Primitives.NetworkType.Public =>
                        Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Public,
                        Swisschain.Sirius.Sdk.Primitives.NetworkType.Test =>
                        Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Test,
                        _ => throw new InvalidEnumArgumentException(
                            nameof(transactionSigningRequest.Blockchain.NetworkType),
                            (int) transactionSigningRequest.Blockchain.NetworkType,
                            typeof(Swisschain.Sirius.Sdk.Primitives.NetworkType))
                    },
                    ProtocolCode = transactionSigningRequest.Blockchain.ProtocolCode,
                    DoubleSpendingProtectionType =
                        transactionSigningRequest.Blockchain.DoubleSpendingProtectionType switch
                        {
                            Swisschain.Sirius.Sdk.Primitives.DoubleSpendingProtectionType.Nonce =>
                            DoubleSpendingProtectionType.Nonce,
                            Swisschain.Sirius.Sdk.Primitives.DoubleSpendingProtectionType.Coins =>
                            DoubleSpendingProtectionType.Coins,
                            _ => throw new InvalidEnumArgumentException(
                                nameof(transactionSigningRequest.Blockchain.DoubleSpendingProtectionType),
                                (int) transactionSigningRequest.Blockchain.DoubleSpendingProtectionType,
                                typeof(Swisschain.Sirius.Sdk.Primitives.DoubleSpendingProtectionType))
                        },
                    BuiltTransaction = ByteString.CopyFrom(transactionSigningRequest.BuiltTransaction.ToArray()),
                    CoinsToSpend =
                    {
                        transactionSigningRequest.CoinsToSpend
                            .Select(coinToSpend =>
                                new CoinToSpend
                                {
                                    Id = new CoinId
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
                    Document = transactionSigningRequest.Document,
                    Signature = transactionSigningRequest.Signature,
                    CreatedAt = Timestamp.FromDateTime(transactionSigningRequest.CreatedAt.UtcDateTime),
                    UpdatedAt = Timestamp.FromDateTime(transactionSigningRequest.UpdatedAt.UtcDateTime),
                    TenantId = transactionSigningRequest.TenantId
                };

                var signingAddresses = transactionSigningRequest.SigningAddresses
                    .Select(o => new SigningAddress
                    {
                        Address = o,
                        Group = transactionSigningRequest.Group
                    });

                item.SigningAddresses.AddRange(signingAddresses);
                response.Requests.Add(item);
            }

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

            var transactionSigningRequest = await _transferSigningRequestsRepository
                .GetByIdAsync(request.TransferSigningRequestId);

            var response = await _vaultAgentClient.TransferSigningRequests.ConfirmAsync(
                new Swisschain.Sirius.VaultAgent.ApiContract.TransferSigningRequests.
                    ConfirmTransferSigningRequestRequest
                    {
                        RequestId = StringUtils.FormatRequestId(transactionSigningRequest.TenantId, request.RequestId),
                        TransactionSigningRequestId = request.TransferSigningRequestId,
                        SignedTransaction = request.SignedTransaction,
                        TransactionId = request.TransactionId,
                        // TODO: HostProcessId = request.HostProcessId
                    });

            if (response.BodyCase == Swisschain.Sirius.VaultAgent.ApiContract.TransferSigningRequests
                .ConfirmTransferSigningRequestResponse.BodyOneofCase.Error)
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

            var transactionSigningRequest = await _transferSigningRequestsRepository
                .GetByIdAsync(request.TransferSigningRequestId);

            var response = await _vaultAgentClient.TransferSigningRequests.RejectAsync(
                new Swisschain.Sirius.VaultAgent.ApiContract.TransferSigningRequests.RejectTransferSigningRequestRequest
                {
                    RequestId = StringUtils.FormatRequestId(transactionSigningRequest.TenantId, request.RequestId),
                    TransactionSigningRequestId = request.TransferSigningRequestId,
                    Reason = request.RejectionReason switch
                    {
                        TransferSigningRequestRejectionReason.UnknownBlockchain => Swisschain.Sirius.VaultAgent
                            .ApiContract.TransferSigningRequests.TransferSigningRequestRejectionReason
                            .UnknownBlockchain,
                        TransferSigningRequestRejectionReason.Other => Swisschain.Sirius.VaultAgent.ApiContract
                            .TransferSigningRequests.TransferSigningRequestRejectionReason
                            .Other,
                        _ => throw new InvalidEnumArgumentException(
                            nameof(request.RejectionReason),
                            (int) request.RejectionReason,
                            typeof(TransferSigningRequestRejectionReason))
                    },
                    RejectionReasonMessage = request.RejectionReasonMessage,
                    // TODO: HostProcessId = request.HostProcessId
                });

            if (response.BodyCase == Swisschain.Sirius.VaultAgent.ApiContract.TransferSigningRequests
                .RejectTransferSigningRequestResponse.BodyOneofCase.Error)
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
