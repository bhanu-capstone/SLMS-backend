using InventoryService.Models;
using InventoryService.Repositories;

namespace InventoryService.Services
{
    public class EntitlementService : IEntitlementService
    {
        private readonly IEntitlementRepository _repo;
        private readonly ILicenseRepository _licenseRepo;

        public EntitlementService(
            IEntitlementRepository repo,
            ILicenseRepository licenseRepo)
        {
            _repo = repo;
            _licenseRepo = licenseRepo;
        }

        public Task<List<Entitlement>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Entitlement?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<Entitlement> AssignAsync(Entitlement entitlement)
        {
            var license = await _licenseRepo.GetByIdAsync(entitlement.LicenseId)
                ?? throw new Exception("License not found");

            // Prevent overuse
            int count = await _repo.CountAssignmentsForLicense(entitlement.LicenseId);
            if (count >= license.TotalEntitlements)
                throw new Exception("No entitlements available");

            // Create entitlement
            await _repo.AddAsync(entitlement);

            // Update assigned count
            license.Assigned = count + 1;
            await _licenseRepo.UpdateAsync(license);

            return entitlement;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ent = await _repo.GetByIdAsync(id);
            if (ent == null) return false;

            var license = await _licenseRepo.GetByIdAsync(ent.LicenseId);
            if (license == null) return false;

            await _repo.DeleteAsync(ent);

            // Recompute assigned count
            int count = await _repo.CountAssignmentsForLicense(ent.LicenseId);
            license.Assigned = count;
            await _licenseRepo.UpdateAsync(license);

            return true;
        }

    }
}
