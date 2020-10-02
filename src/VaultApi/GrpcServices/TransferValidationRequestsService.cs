using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore.Internal;
using Swisschain.Sirius.Sdk.Primitives;
using Swisschain.Sirius.VaultApi.ApiContract;
using Swisschain.Sirius.VaultApi.ApiContract.Common;
using Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests;
using VaultApi.Common.Persistence.TransferValidationRequests;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.ReadModels.TransferValidationRequests;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;
using Asset = Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.Asset;
using DestinationAddress = Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.DestinationAddress;
using NetworkType = Swisschain.Sirius.Sdk.Primitives.NetworkType;
using SourceAddress = Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.SourceAddress;
using TransferDetails = Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.TransferDetails;

namespace VaultApi.GrpcServices
{
    public class TransferValidationRequestsService : TransferValidationRequests.TransferValidationRequestsBase
    {
        private readonly ITransferValidationRequestRepository _transferValidationRequestRepository;
        private readonly IVaultsRepository _vaultsRepository;
        private readonly Swisschain.Sirius.VaultAgent.ApiClient.IVaultAgentClient _vaultAgentClient;

        public TransferValidationRequestsService(
            ITransferValidationRequestRepository transferValidationRequestRepository,
            IVaultsRepository vaultsRepository,
            Swisschain.Sirius.VaultAgent.ApiClient.IVaultAgentClient vaultAgentClient)
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

            IReadOnlyCollection<VaultApi.Common.ReadModels.TransferValidationRequests.TransferValidationRequest> requests = null;
            if (vaultType == VaultType.Shared)
                requests = await _transferValidationRequestRepository.GetPendingForSharedVaultAsync();
            else
                requests = await _transferValidationRequestRepository.GetPendingForPrivateVaultAsync(vaultId.Value);

            var response = new GetTransferValidationRequestsResponse()
            {
                Response = new GetTransferValidationRequestsResponseBody()
                {
                }
            };

            if (requests.Any())
                response.Response.Requests.AddRange(requests.Select(x => new Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests.TransferValidationRequest()
                {
                    CreatedAt = Timestamp.FromDateTimeOffset(x.CreatedAt),
                    CustomerSignature = x.CustomerSignature,
                    Details = new Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests.TransferDetails()
                    {
                        Amount = x.Details.Amount,
                        Asset = new Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests.Asset()
                        {
                            Address = x.Details.Asset.Address,
                            Id = x.Details.Asset.Id,
                            Symbol = x.Details.Asset.Symbol
                        },
                        Blockchain = new Blockchain()
                        {
                            Id = x.Details.BlockchainId,
                            NetworkType = x.Details.NetworkType switch
                            {
                                NetworkType.Private => Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Private,
                                NetworkType.Test => Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Test,
                                NetworkType.Public => Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Public,
                                _ => throw new ArgumentOutOfRangeException(nameof(x.Details.NetworkType), x.Details.NetworkType, null)
                            },
                            ProtocolId = x.Details.ProtocolId
                        },
                        ClientContext = new ClientContext()
                        {
                            ApiKeyId = x.Details.UserContext.ApiKeyId,
                            WithdrawalReferenceId = x.Details.UserContext.WithdrawalReferenceId,
                            AccountReferenceId = x.Details.UserContext.AccountReferenceId,
                            Ip = x.Details.UserContext.PassClientIp,
                            UserId = x.Details.UserContext.UserId
                        },
                        DestinationAddress = new Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests.DestinationAddress()
                        {
                            Name = x.Details.DestinationAddress.Name,
                            Group = x.Details.DestinationAddress.Group,
                            Address = x.Details.DestinationAddress.Address,
                            Tag = x.Details.DestinationAddress.Tag,
                            TagType = !x.Details.DestinationAddress.TagType.HasValue ? new NullableTagType() { Null = NullValue.NullValue } :
                                new NullableTagType()
                                {
                                    TagType = x.Details.DestinationAddress.TagType.Value switch
                                    {
                                        DestinationTagType.Text => TagType.Text,
                                        DestinationTagType.Number => TagType.Number,
                                        _ => throw new ArgumentOutOfRangeException(nameof(x.Details.DestinationAddress.TagType.Value),
                                            x.Details.DestinationAddress.TagType.Value, null)
                                    }
                                }
                        },
                        FeeLimit = x.Details.FeeLimit,
                        OperationId = x.Details.OperationId,
                        SourceAddress = new Swisschain.Sirius.VaultApi.ApiContract.TransferValidationRequests.SourceAddress()
                        {
                            Address = x.Details.SourceAddress.Address,
                            Group = x.Details.SourceAddress.Group,
                            Name = x.Details.SourceAddress.Name
                        }
                    },
                    Id = x.Id,
                    SiriusSignature = x.SiriusSignature,
                    UpdatedAt = Timestamp.FromDateTimeOffset(x.UpdatedAt),
                    TenantId = x.TenantId
                }).ToArray());

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
                new Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.ConfirmTransferValidationRequestRequest()
                {
                    TransferValidationRequestId = transferValidationRequest.Id,
                    HostProcessId = request.HostProcessId,
                    PolicyResult = request.PolicyResult,
                    RequestId = StringUtils.FormatRequestId(transferValidationRequest.TenantId, request.RequestId),
                    Signature = request.Signature
                });

            if (result.BodyCase == Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.ConfirmTransferValidationRequestResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(ConfirmTransferValidationRequestErrorResponseBody.Types.ErrorCode.Unknown, result.Error.ErrorMessage);
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
                new Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.RejectTransferValidationRequestRequest()
                {
                    TransferValidationRequestId = transferValidationRequest.Id,
                    HostProcessId = request.HostProcessId,
                    PolicyResult = request.PolicyResult,
                    RequestId = StringUtils.FormatRequestId(transferValidationRequest.TenantId, request.RequestId),
                    Signature = request.Signature
                });

            if (result.BodyCase == 
                Swisschain.Sirius.VaultAgent.ApiContract.TransferValidationRequests.RejectTransferValidationRequestResponse.BodyOneofCase.Error)
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
