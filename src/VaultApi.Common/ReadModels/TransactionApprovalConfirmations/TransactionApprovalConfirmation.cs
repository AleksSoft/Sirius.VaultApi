using System;

namespace VaultApi.Common.ReadModels.TransactionApprovalConfirmations
{
    public class TransactionApprovalConfirmation
    {
        public long Id { get; set; }

        public string TenantId { get; set; }

        public long TransactionApprovalRequestId { get; set; }

        public string KeyId { get; set; }

        public string Message { get; set; }

        public string Secret { get; set; }

        public TransactionApprovalConfirmationStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }        
    }
}
