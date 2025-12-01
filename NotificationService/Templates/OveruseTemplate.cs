namespace NotificationService.Templates
{
    public static class OveruseTemplate
    {
        public static string Build(string product, int used, int entitlements)
        {
            return $"Overuse alert for {product}.\nUsed: {used}\nEntitlements: {entitlements}\n\nPlease investigate.";
        }
    }
}
