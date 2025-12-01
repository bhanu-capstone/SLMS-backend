namespace NotificationService.Models
{
    public class ComplianceEventDto
    {
        public int Id { get; set; }
        public int LicenseId { get; set; }
        public string ProductName { get; set; } = "";
        public string EventType { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
