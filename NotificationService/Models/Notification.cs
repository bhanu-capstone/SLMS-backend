using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string ToEmail { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public string Type { get; set; } = "";
        public string Channel { get; set; } = "email"; // e.g., email, sms, push
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DesiredSendAt { get; set; }
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; } = 0;
    }
}
