using System;
using System.Collections.Generic;
using Swisschain.Sirius.Sdk.Primitives;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.ReadModels.Transactions
{
    public class TransactionSigningRequest
    {
        public long Id { get; set; }

        public string TenantId { get; set; }

        public string Group { get; set; }

        public long VaultId { get; set; }

        public VaultType VaultType { get; set; }

        public string Component { get; set; }

        public long OperationId { get; set; }

        public string OperationType { get; set; }

        public string BlockchainId { get; set; }

        public TransactionSigningRequestState State { get; set; }

        public string RejectionReasonMessage { get; set; }

        public TransactionRejectionReason? RejectionReason { get; set; }

        public NetworkType NetworkType { get; set; }

        public string ProtocolCode { get; set; }

        public DoubleSpendingProtectionType DoubleSpendingProtectionType { get; set; }

        public byte[] BuiltTransaction { get; set; }

        public IReadOnlyCollection<string> SigningAddresses { get; set; }

        public IReadOnlyCollection<Coin> CoinsToSpend { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public UserContext UserContext { get; set; }
    }
}
