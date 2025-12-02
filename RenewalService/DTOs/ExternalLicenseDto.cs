namespace RenewalService.DTOs
{
    public class ExternalLicenseDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Vendor { get; set; }
        public int TotalEntitlements { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal Cost { get; set; }

    }
}
