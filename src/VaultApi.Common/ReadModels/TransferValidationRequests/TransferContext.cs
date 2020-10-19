namespace VaultApi.Common.ReadModels.TransferValidationRequests
{
    public class TransferContext
    {
        public string AccountReferenceId { get; set; }

        public string WithdrawalReferenceId { get; set; }

        public string Component { get; set; }

        public string OperationType { get; set; }

        public string SourceGroup { get; set; }

        public string DestinationGroup { get; set; }

        public string Document { get; set; }

        public string Signature { get; set; }

        public RequestContext RequestContext { get; set; }
    }
}
