using System;
using VaultApi.Common.ReadModels.Vaults;

namespace VaultApi.Common.ReadModels.TransferValidationRequests
{
    public class TransferValidationRequest
    {
        public long Id { get; set; }
        public TransferDetails Details { get; set; }
        public string CustomerSignature { get; set; }
        public string SiriusSignature { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public TransferValidationRequestState State { get; set; }
        public long Sequence { get; set; }
        public TransferValidationRequestRejectionReason? RejectionReason { get; set; }
        public string RejectionReasonMessage { get; set; }
        public long VaultId { get; set; }
        public VaultType VaultType { get; set; }

        public string TenantId { get; set; }
    }
}
