using InventoryService.Models;
using InventoryService.Repositories;

namespace InventoryService.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ILicenseRepository _repo;
        private readonly IEntitlementRepository _entitlementRepo;

        public LicenseService(ILicenseRepository repo, IEntitlementRepository entitlementRepo)
        {
            _repo = repo;
            _entitlementRepo = entitlementRepo;
        }

        public Task<List<License>> GetAllAsync() => _repo.GetAllAsync();

        public Task<License?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<License> CreateAsync(License license)
        {
            await _repo.AddAsync(license);
            return license;
        }

        public async Task<License> UpdateAsync(int id, License update)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception("License not found");

            existing.ProductName = update.ProductName;
            existing.Vendor = update.Vendor;
            existing.SKU = update.SKU;
            existing.LicenseType = update.LicenseType;
            existing.TotalEntitlements = update.TotalEntitlements;
            existing.Cost = update.Cost;
            existing.Currency = update.Currency;
            existing.ExpiryDate = update.ExpiryDate;
            existing.PurchaseDate = update.PurchaseDate;
            existing.Notes = update.Notes;

            await _repo.UpdateAsync(existing);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var license = await _repo.GetByIdAsync(id);
            if (license == null) return false;

            await _repo.DeleteAsync(license);
            return true;
        }

        public Task<List<License>> SearchAsync(string? product, string? vendor) =>
            _repo.SearchAsync(product, vendor);

        /// <summary>
        /// Recalculate Assigned count = number of active entitlements
        /// </summary>
        public async Task UpdateAssignedCountAsync(int licenseId)
        {
            var ents = await _entitlementRepo.GetAllAsync();
            var count = ents.Count(e => e.LicenseId == licenseId);

            var lic = await _repo.GetByIdAsync(licenseId);
            if (lic == null) return;

            lic.Assigned = count;
            await _repo.UpdateAsync(lic);
        }
        public async Task<List<License>> GetExpiringLicensesAsync(int days)
        {
            if (days < 0) throw new ArgumentException("Days cannot be negative");

            // You can add extra business logic here if needed.
            // For example: Filter out licenses that are marked "Do Not Renew" 
            // or send an alert log if too many licenses are expiring at once.

            return await _repo.GetExpiringLicensesAsync(days);
        }
    }
}
