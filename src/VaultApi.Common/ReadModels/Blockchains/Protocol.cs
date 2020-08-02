using Swisschain.Sirius.Sdk.Primitives;

namespace VaultApi.Common.ReadModels.Blockchains
{
    public class Protocol
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public long StartBlockNumber { get; set; }

        public DoubleSpendingProtectionType DoubleSpendingProtectionType { get; set; }
    }
}
