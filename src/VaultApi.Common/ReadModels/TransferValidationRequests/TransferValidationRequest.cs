using System;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.ReadModels.TransferValidationRequests
{
    public class TransferValidationRequest
    {
        public long Id { get; set; }

        public long TransferId { get; set; }

        public string TenantId { get; set; }

        public long VaultId { get; set; }

        public VaultType VaultType { get; set; }

        public Blockchain Blockchain { get; set; }

        public Asset Asset { get; set; }

        public SourceAddress SourceAddress { get; set; }

        public DestinationAddress DestinationAddress { get; set; }

        public decimal Amount { get; set; }

        public decimal FeeLimit { get; set; }

        public TransferContext TransferContext { get; set; }

        public string Document { get; set; }

        public string Signature { get; set; }

        public TransferValidationRequestRejectionReason? RejectionReason { get; set; }

        public string RejectionReasonMessage { get; set; }

        public TransferValidationRequestState State { get; set; }

        public long Sequence { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
