using System;
using Swisschain.Sirius.Sdk.Primitives;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.ReadModels.Wallets
{
    public class WalletGenerationRequest
    {
        public long Id { get; set; }

        public string TenantId { get; set; }

        public long VaultId { get; set; }

        public VaultType VaultType { get; set; }

        public string BlockchainId { get; set; }

        public NetworkType NetworkType { get; set; }

        public string ProtocolCode { get; set; }

        public string Component { get; set; }

        public WalletGenerationRequestState State { get; set; }

        public WalletRejectionReason? RejectionReason { get; set; }

        public string RejectionReasonMessage { get; set; }

        public string Group { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
