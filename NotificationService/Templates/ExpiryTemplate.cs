namespace NotificationService.Templates
{
    public static class ExpiryTemplate
    {
        public static string Build(string product, string sku, DateTime expiry, int offset, string unit)
        {
            return $"Dear owner,\n\nThe entitlement {sku} for {product} is expiring on {expiry:yyyy-MM-dd HH:mm:ss}. This is a notification {offset} {unit} before expiry.\n\nPlease review renewals or reclaim seats if needed.\n\nRegards,\nSLMS";
        }
    }
}
