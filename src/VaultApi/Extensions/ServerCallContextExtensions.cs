using System;
using System.Security.Claims;
using Grpc.Core;
using Swisschain.Sdk.Server.Authorization;
using VaultApi.Common;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Extensions
{
    public static class ServerCallContextExtensions
    {
        public static string GetTenantId(this ServerCallContext context)
        {
            return context.GetHttpContext().User.GetTenantIdOrDefault();
        }

        public static long? GetVaultId(this ServerCallContext context)
        {
            var vaultIdClaim = context.GetHttpContext().User.GetClaimOrDefault(VaultTokenClaims.VaultId);

            if (string.IsNullOrEmpty(vaultIdClaim))
                return null;

            if (!long.TryParse(vaultIdClaim, out var vaultId))
                return null;

            return vaultId;
        }

        public static VaultType? GetVaultType(this ServerCallContext context)
        {
            var vaultTypeClaim = context.GetHttpContext().User.GetClaimOrDefault(VaultTokenClaims.VaultType);

            if (string.IsNullOrEmpty(vaultTypeClaim))
                return null;

            if (!Enum.TryParse(typeof(VaultType), vaultTypeClaim, true, out var vaultType))
                return null;

            return (VaultType) vaultType;
        }
    }
}
