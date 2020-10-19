using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Swisschain.Sirius.Sdk.Primitives;
using Swisschain.Sirius.VaultApi.ApiContract.Common;
using Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests;
using VaultApi.Common.Persistence.TransferValidationRequests;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;

namespace VaultApi.GrpcServices
{
    public class TransferValidationRequestsService : TransferValidationRequests.TransferValidationRequestsBase
    {
        private readonly ITransferValidationRequestRepository _transferValidationRequestRepository;
        private readonly Swisschain.Sirius.VaultAgent.ApiClient.IVaultAgentClient _vaultAgentClient;

        public TransferValidationRequestsService(
            ITransferValidationRequestRepository transferValidationRequestRepository,
            Swisschain.Sirius.VaultAgent.ApiClient.IVaultAgentClient vaultAgentClient)
        {
            _transferValidationRequestRepository = transferValidationRequestRepository;
            _vaultAgentClient = vaultAgentClient;
        }

        public override async Task<GetTransferValidationRequestsResponse> Get(
            GetTransferValidationRequestsRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    GetTransferValidationRequestsErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    GetTransferValidationRequestsErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            IReadOnlyCollection<VaultApi.Common.ReadModels.TransferValidationRequests.TransferValidationRequest>
                requests;
            if (vaultType == VaultType.Shared)
                requests = await _transferValidationRequestRepository.GetPendingForSharedVaultAsync();
            else
                requests = await _transferValidationRequestRepository.GetPendingForPrivateVaultAsync(vaultId.Value);

            var response = new GetTransferValidationRequestsResponse
            {
                Response = new GetTransferValidationRequestsResponseBody()
            };

            if (requests.Any())
                response.Response.Requests.AddRange(requests.Select(x => new TransferValidationRequest
                    {
                        Id = x.Id,
                        OperationId = x.TransferId,
                        TenantId = x.TenantId,
                        Blockchain = new Blockchain
                        {
                            Id = x.Blockchain.Id,
                            NetworkType = x.Blockchain.NetworkType switch
                            {
                                Swisschain.Sirius.Sdk.Primitives.NetworkType.Private =>
                                Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Private,
                                Swisschain.Sirius.Sdk.Primitives.NetworkType.Test =>
                                Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Test,
                                Swisschain.Sirius.Sdk.Primitives.NetworkType.Public =>
                                Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Public,
                                _ => throw new ArgumentOutOfRangeException(nameof(x.Blockchain.NetworkType),
                                    x.Blockchain.NetworkType,
                                    null)
                            },
                            ProtocolId = x.Blockchain.ProtocolCode
                        },
                        Asset = new Asset
                        {
                            Address = x.Asset.Address,
                            Id = x.Asset.Id,
                            Symbol = x.Asset.Symbol
                        },
                        SourceAddress = new SourceAddress
                        {
                            Address = x.SourceAddress.Address,
                            Group = x.SourceAddress.Group,
                            Name = x.SourceAddress.Name
                        },
                        DestinationAddress = new DestinationAddress
                        {
                            Name = x.DestinationAddress.Name,
                            Group = x.DestinationAddress.Group,
                            Address = x.DestinationAddress.Address,
                            Tag = x.DestinationAddress.Tag,
                            TagType = !x.DestinationAddress.TagType.HasValue
                                ? new NullableTagType {Null = NullValue.NullValue}
                                : new NullableTagType
                                {
                                    TagType = x.DestinationAddress.TagType.Value switch
                                    {
                                        DestinationTagType.Text => TagType.Text,
                                        DestinationTagType.Number => TagType.Number,
                                        _ => throw new ArgumentOutOfRangeException(
                                            nameof(x.DestinationAddress.TagType.Value),
                                            x.DestinationAddress.TagType.Value,
                                            null)
                                    }
                                }
                        },
                        Amount = x.Amount,
                        FeeLimit = x.FeeLimit,
                        TransferContext = new TransferContext
                        {
                            AccountReferenceId = x.TransferContext.AccountReferenceId,
                            WithdrawalReferenceId = x.TransferContext.WithdrawalReferenceId,
                            Component = x.TransferContext.Component,
                            OperationType = x.TransferContext.OperationType,
                            SourceGroup = x.TransferContext.SourceGroup,
                            DestinationGroup = x.TransferContext.DestinationGroup,
                            Document = x.TransferContext.Document,
                            Signature = x.TransferContext.Signature,
                            RequestContext = new RequestContext
                            {
                                UserId = x.TransferContext.RequestContext.UserId,
                                ApiKeyId = x.TransferContext.RequestContext.ApiKeyId,
                                Ip = x.TransferContext.RequestContext.Ip,
                                Timestamp = new NullableTimestamp
                                {
                                    Timestamp = Timestamp.FromDateTimeOffset(x.TransferContext.RequestContext.Timestamp)
                                }
                            }
                        },
                        CreatedAt = Timestamp.FromDateTimeOffset(x.CreatedAt),
                        UpdatedAt = Timestamp.FromDateTimeOffset(x.UpdatedAt),
                    })
                    .ToArray());

            return response;
        }

        public override async Task<ConfirmTransferValidationRequestResponse> Confirm(
            ConfirmTransferValidationRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    ConfirmTransferValidationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    ConfirmTransferValidationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var transferValidationRequest = await _transferValidationRequestRepository
                .GetByIdAsync(request.TransferValidationRequestId);

            var result = await _vaultAgentClient.TransferValidationRequests.ConfirmAsync(
                new Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.
                    ConfirmTransferValidationRequestRequest
                    {
                        RequestId = StringUtils.FormatRequestId(transferValidationRequest.TenantId, request.RequestId),
                        TransferValidationRequestId = transferValidationRequest.Id,
                        Document = request.Document,
                        Signature = request.Signature,
                        HostProcessId = request.HostProcessId
                    });

            if (result.BodyCase == Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests
                .ConfirmTransferValidationRequestResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(ConfirmTransferValidationRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    result.Error.ErrorMessage);
            }

            return new ConfirmTransferValidationRequestResponse
            {
                Response = new ConfirmTransferValidationRequestResponseBody()
            };
        }

        public override async Task<RejectTransferValidationRequestResponse> Reject(
            RejectTransferValidationRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    RejectTransferValidationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    RejectTransferValidationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var transferValidationRequest = await _transferValidationRequestRepository
                .GetByIdAsync(request.TransferValidationRequestId);

            var result = await _vaultAgentClient.TransferValidationRequests.RejectAsync(
                new Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.
                    RejectTransferValidationRequestRequest
                    {
                        RequestId = StringUtils.FormatRequestId(transferValidationRequest.TenantId, request.RequestId),
                        TransferValidationRequestId = transferValidationRequest.Id,
                        RejectionReason = request.RejectionReason switch
                        {
                            TransferValidationRequestRejectionReason.Other => Swisschain.Sirius.VaultAgent.ApiContract
                                .TransferValidationRequests.TransferValidationRequestRejectionReason.Other,
                            TransferValidationRequestRejectionReason.RejectedByPolicy => Swisschain.Sirius.VaultAgent
                                .ApiContract.TransferValidationRequests.TransferValidationRequestRejectionReason
                                .RejectedByPolicy,
                            _ => throw new ArgumentOutOfRangeException(nameof(request.RejectionReason),
                                request.RejectionReason,
                                null)
                        },
                        RejectionReasonMessage = request.RejectionReasonMessage,
                        Document = request.Document,
                        Signature = request.Signature,
                        HostProcessId = request.HostProcessId
                    });

            if (result.BodyCase ==
                Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests
                    .RejectTransferValidationRequestResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(RejectTransferValidationRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    result.Error.ErrorMessage);
            }

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
