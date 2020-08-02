namespace VaultApi.Utils
{
    public static class StringUtils
    {
        public static string FormatRequestId(string tenantId, string requestId)
        {
            return $"VaultApi:{tenantId}:{requestId}";
        }
    }
}
