using System;
using System.Collections.Generic;
using System.Text;

namespace VaultApi.Common.ReadModels.Wallets
{
    public class WalletGenerationContext
    {
        public WalletGenerationContextObjectType ObjectType { get; set; }

        public long ObjectId { get; set; }

        public string ReferenceId { get; set; }
    }
}
