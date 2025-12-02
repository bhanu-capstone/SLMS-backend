using System.ComponentModel.DataAnnotations;

namespace RenewalService.Models
{
    public class RenewalRecord
    {
        [Key]
        public int Id { get; set; }
        public int LinkedLicenseId { get; set; } // Link to Inventory
        public string VendorName { get; set; } = "";
        public DateTime TargetExpiryDate { get; set; }
        public string Status { get; set; } = "Open"; // Open, InProgress, Renewed
        public string AdminNotes { get; set; } = "";
        public DateTime? LastReminderSent { get; set; }
    }
}
