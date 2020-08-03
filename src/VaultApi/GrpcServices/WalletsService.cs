﻿using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Swisschain.Sirius.VaultAgent.ApiClient;
using Swisschain.Sirius.VaultAgent.ApiContract.Wallets;
using Swisschain.Sirius.VaultApi.ApiContract.Wallets;
using VaultApi.Common.Persistence.Wallets;
using VaultApi.Common.ReadModels.Vaults;
using VaultApi.Extensions;
using VaultApi.Utils;

namespace VaultApi.GrpcServices
{
    [Authorize]
    public class WalletsService : Swisschain.Sirius.VaultApi.ApiContract.Wallets.Wallets.WalletsBase
    {
        private readonly IWalletGenerationRequestRepository _walletGenerationRequestRepository;
        private readonly IVaultAgentClient _vaultAgentClient;

        public WalletsService(
            IWalletGenerationRequestRepository walletGenerationRequestRepository,
            IVaultAgentClient vaultAgentClient)
        {
            _walletGenerationRequestRepository = walletGenerationRequestRepository;
            _vaultAgentClient = vaultAgentClient;
        }

        public override async Task<GetWalletGenerationRequestResponse> Get(GetWalletGenerationRequestRequest request,
            ServerCallContext context)
        {
            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(GetWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(GetWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var pendingWalletGenerationRequests = vaultType == VaultType.Shared
                ? await _walletGenerationRequestRepository.GetPendingForSharedVaultAsync()
                : await _walletGenerationRequestRepository.GetPendingForPrivateVaultAsync(vaultId.Value);

            var response = new GetWalletGenerationRequestResponseBody();

            response.Requests.AddRange(pendingWalletGenerationRequests
                .Select(walletGenerationRequest => new WalletGenerationRequest
                {
                    Id = walletGenerationRequest.Id,
                    BlockchainId = walletGenerationRequest.BlockchainId,
                    NetworkType = walletGenerationRequest.NetworkType switch
                    {
                        Swisschain.Sirius.Sdk.Primitives.NetworkType.Private =>
                        Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Private,
                        Swisschain.Sirius.Sdk.Primitives.NetworkType.Public =>
                        Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Public,
                        Swisschain.Sirius.Sdk.Primitives.NetworkType.Test =>
                        Swisschain.Sirius.VaultApi.ApiContract.Common.NetworkType.Test,
                        _ => throw new InvalidEnumArgumentException(nameof(walletGenerationRequest.NetworkType),
                            (int) walletGenerationRequest.NetworkType,
                            typeof(Swisschain.Sirius.Sdk.Primitives.NetworkType))
                    },
                    ProtocolCode = walletGenerationRequest.ProtocolCode,
                    CreatedAt = Timestamp.FromDateTime(walletGenerationRequest.CreatedAt.UtcDateTime),
                    UpdatedAt = Timestamp.FromDateTime(walletGenerationRequest.UpdatedAt.UtcDateTime)
                }));

            return new GetWalletGenerationRequestResponse {Response = response};
        }

        public override async Task<ConfirmWalletGenerationRequestResponse> Confirm(
            ConfirmWalletGenerationRequestRequest request,
            ServerCallContext context)
        {
            var tenantId = context.GetTenantId();

            if (string.IsNullOrEmpty(tenantId))
            {
                return GetErrorResponse(
                    ConfirmWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Tenant id required");
            }

            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    ConfirmWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    ConfirmWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var response = await _vaultAgentClient.Wallets.ConfirmAsync(new ConfirmWalletRequest
            {
                RequestId = StringUtils.FormatRequestId(tenantId, request.RequestId),
                TenantId = tenantId,
                WalletGenerationRequestId = request.WalletGenerationRequestId,
                Address = request.Address,
                PublicKey = request.PublicKey
            });

            if (response.BodyCase == ConfirmWalletResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    ConfirmWalletGenerationRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    response.Error.ErrorMessage);
            }

            return new ConfirmWalletGenerationRequestResponse
            {
                Response = new ConfirmWalletGenerationRequestResponseBody()
            };
        }

        public override async Task<RejectWalletGenerationRequestResponse> Reject(
            RejectWalletGenerationRequestRequest request,
            ServerCallContext context)
        {
            var tenantId = context.GetTenantId();

            if (string.IsNullOrEmpty(tenantId))
            {
                return GetErrorResponse(
                    RejectWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Tenant id required");
            }

            var vaultType = context.GetVaultType();

            if (!vaultType.HasValue)
            {
                return GetErrorResponse(
                    RejectWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Vault type required");
            }

            var vaultId = context.GetVaultId();

            if (!vaultId.HasValue && vaultType == VaultType.Private)
            {
                return GetErrorResponse(
                    RejectWalletGenerationRequestErrorResponseBody.Types.ErrorCode.InvalidParameters,
                    "Private vault id required");
            }

            var response = await _vaultAgentClient.Wallets.RejectAsync(new RejectWalletRequest
            {
                RequestId = StringUtils.FormatRequestId(tenantId, request.RequestId),
                TenantId = tenantId,
                WalletGenerationRequestId = request.WalletGenerationRequestId,
                Reason = request.Reason switch
                {
                    Swisschain.Sirius.VaultApi.ApiContract.Wallets.RejectionReason.Other =>
                    Swisschain.Sirius.VaultAgent.ApiContract.Wallets.RejectionReason.Other,
                    Swisschain.Sirius.VaultApi.ApiContract.Wallets.RejectionReason.UnknownBlockchain =>
                    Swisschain.Sirius.VaultAgent.ApiContract.Wallets.RejectionReason.UnknownBlockchain,
                    _ => throw new InvalidEnumArgumentException(
                        nameof(request.Reason),
                        (int) request.Reason,
                        typeof(Swisschain.Sirius.VaultApi.ApiContract.Wallets.RejectionReason))
                },
                ReasonMessage = request.ReasonMessage
            });

            if (response.BodyCase == RejectWalletResponse.BodyOneofCase.Error)
            {
                return GetErrorResponse(
                    RejectWalletGenerationRequestErrorResponseBody.Types.ErrorCode.Unknown,
                    response.Error.ErrorMessage);
            }

            return new RejectWalletGenerationRequestResponse
            {
                Response = new RejectWalletGenerationRequestResponseBody()
            };
        }

        private static GetWalletGenerationRequestResponse GetErrorResponse(
            GetWalletGenerationRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new GetWalletGenerationRequestResponse
            {
                Error = new GetWalletGenerationRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static ConfirmWalletGenerationRequestResponse GetErrorResponse(
            ConfirmWalletGenerationRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new ConfirmWalletGenerationRequestResponse
            {
                Error = new ConfirmWalletGenerationRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }

        private static RejectWalletGenerationRequestResponse GetErrorResponse(
            RejectWalletGenerationRequestErrorResponseBody.Types.ErrorCode errorCode,
            string message)
        {
            return new RejectWalletGenerationRequestResponse
            {
                Error = new RejectWalletGenerationRequestErrorResponseBody
                {
                    ErrorCode = errorCode,
                    ErrorMessage = message
                }
            };
        }
    }
}
