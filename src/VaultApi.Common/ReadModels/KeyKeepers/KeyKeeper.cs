using System;

namespace VaultApi.Common.ReadModels.KeyKeepers
{
    public class KeyKeeper
    {
        public long Id { get; set; }

        public string TenantId { get; set; }

        public string KeyId { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
