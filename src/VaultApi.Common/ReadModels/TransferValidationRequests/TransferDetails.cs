using Swisschain.Sirius.Sdk.Primitives;
using VaultApi.Common.ReadModels.Transactions;

namespace VaultApi.Common.ReadModels.TransferValidationRequests
{
    public class TransferDetails
    {
        public long OperationId { get; set; }
        public string BlockchainId { get; set; }

        public NetworkType NetworkType { get; set; }

        public string ProtocolId { get; set; }
        public Asset Asset { get; set; }
        public SourceAddress SourceAddress { get; set; }
        public DestinationAddress DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeLimit { get; set; }
        public UserContext UserContext { get; set; }
    }
}
