using RenewalService.DTOs;
using RenewalService.Models;
using RenewalService.Repositories;

namespace RenewalService.Services
{
    public class RenewalManager
    {
        // DEPENDENCIES ARE NOW CLASSES, NOT INTERFACES
        private readonly RenewalRepository _repo;
        private readonly InventoryIntegrationService _inventory;

        public RenewalManager(RenewalRepository repo, InventoryIntegrationService inventory)
        {
            _repo = repo;
            _inventory = inventory;
        }

        public async Task<List<object>> GetExpiringWithStatusAsync(int days)
        {
            var expiringLicenses = await _inventory.GetExpiringLicensesAsync(days);
            var allRenewals = await _repo.GetAllAsync();

            var result = new List<object>();

            foreach (var lic in expiringLicenses)
            {
                var existingWork = allRenewals.FirstOrDefault(r => r.LinkedLicenseId == lic.Id);

                result.Add(new
                {
                    LicenseId = lic.Id,
                    Vendor = lic.Vendor,
                    ProductName = lic.ProductName,
                    ExpiryDate = lic.ExpiryDate,
                    RenewalStatus = existingWork?.Status ?? "Action Required",
                    RenewalId = existingWork?.Id
                });
            }
            return result;
        }

        public async Task<int> SendRemindersAsync(int days)
        {
            var expiring = await _inventory.GetExpiringLicensesAsync(days);
            int sentCount = 0;

            foreach (var lic in expiring)
            {
                var existing = await _repo.GetByLicenseIdAsync(lic.Id);
                if (existing == null)
                {
                    sentCount++;
                    // Send Email Logic here...
                }
            }
            return sentCount;
        }

        public async Task MarkAsRenewedAsync(int id, string note)
        {
            var record = await _repo.GetByIdAsync(id);
            if (record == null) throw new Exception("Record not found");

            record.Status = "Renewed";
            record.AdminNotes += $" || Renewed on {DateTime.Now}: {note}";
            await _repo.UpdateAsync(record);
        }
    }
}