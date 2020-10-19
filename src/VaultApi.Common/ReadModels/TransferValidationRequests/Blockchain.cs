using Swisschain.Sirius.Sdk.Primitives;

namespace VaultApi.Common.ReadModels.TransferValidationRequests
{
    public class Blockchain
    {
        public string Id { get; set; }

        public NetworkType NetworkType { get; set; }

        public string ProtocolCode { get; set; }
    }
}
