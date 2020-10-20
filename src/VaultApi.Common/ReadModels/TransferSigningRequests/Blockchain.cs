using Swisschain.Sirius.Sdk.Primitives;

namespace VaultApi.Common.ReadModels.TransferSigningRequests
{
    public class Blockchain
    {
        public string Id { get; set; }

        public NetworkType NetworkType { get; set; }

        public string ProtocolCode { get; set; }

        public DoubleSpendingProtectionType DoubleSpendingProtectionType { get; set; }
    }
}
