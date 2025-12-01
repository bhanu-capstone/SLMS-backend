namespace InventoryService.Models
{
    public class ExpiringEntitlementDto
    {
        public int EntitlementId { get; set; }
        public string ProductName { get; set; } = "";
        public string LicenseName { get; set; } = "";
        public string? UserId { get; set; }
        public int? DeviceId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string OwnerEmail { get; set; } = "bsai29074@gmail.com"; // FIX LATER
    }
}
    