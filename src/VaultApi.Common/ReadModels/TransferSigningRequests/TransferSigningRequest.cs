using System;
using System.Collections.Generic;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.ReadModels.TransferSigningRequests
{
    public class TransferSigningRequest
    {
        public long Id { get; set; }

        public long TransferId { get; set; }

        public string TenantId { get; set; }

        public long VaultId { get; set; }

        public VaultType VaultType { get; set; }

        public Blockchain Blockchain { get; set; }

        public byte[] BuiltTransaction { get; set; }

        public IReadOnlyCollection<string> SigningAddresses { get; set; }

        public IReadOnlyCollection<Coin> CoinsToSpend { get; set; }

        public TransferRejectionReason? RejectionReason { get; set; }

        public string RejectionReasonMessage { get; set; }

        public TransferSigningRequestState State { get; set; }

        public string Document { get; set; }

        public string Signature { get; set; }

        public string Group { get; set; }

        public long Sequence { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
