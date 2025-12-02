using System.Text.Json.Serialization;

namespace ReportingService.Models
{
    public class LicenseDto
    {
        //changed
        [JsonPropertyName("licenseId")] 
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public string Vendor { get; set; } = "";
        public int TotalEntitlements { get; set; }
    }

    public class EntitlementDto
    {
        public int LicenseId { get; set; }
    }

    public class InstalledSoftwareDto
    {
        public int? LicenseId { get; set; }
        public string ProductName { get; set; } = "";
        public int DeviceId { get; set; } 
    }
}
