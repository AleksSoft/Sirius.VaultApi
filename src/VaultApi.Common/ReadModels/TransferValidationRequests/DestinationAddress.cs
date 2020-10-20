using Swisschain.Sirius.Sdk.Primitives;

namespace VaultApi.Common.ReadModels.TransferValidationRequests
{
    public class DestinationAddress
    {
        public string Address { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public DestinationTagType? TagType { get; set; }

        public string Tag { get; set; }
    }
}
