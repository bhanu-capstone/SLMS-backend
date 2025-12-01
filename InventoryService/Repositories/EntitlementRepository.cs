using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    public class EntitlementRepository : IEntitlementRepository
    {
        private readonly AppDbContext _db;
        public EntitlementRepository(AppDbContext db) => _db = db;

        public Task<List<Entitlement>> GetAllAsync() =>
            _db.Entitlements
               .Include(e => e.License)
               .Include(e => e.User)
               .Include(e => e.Device)
               .ToListAsync();

        public Task<Entitlement?> GetByIdAsync(int id) =>
            _db.Entitlements
               .Include(e => e.License)
               .Include(e => e.User)
               .Include(e => e.Device)
               .FirstOrDefaultAsync(e => e.Id == id);

        public Task<List<Entitlement>> GetByLicenseIdAsync(int licenseId) =>
            _db.Entitlements
               .Include(e => e.User)
               .Include(e => e.Device)
               .Where(e => e.LicenseId == licenseId)
               .ToListAsync();

        public async Task AddAsync(Entitlement entitlement)
        {
            await _db.Entitlements.AddAsync(entitlement);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Entitlement entitlement)
        {
            _db.Entitlements.Update(entitlement);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Entitlement entitlement)
        {
            _db.Entitlements.Remove(entitlement);
            await _db.SaveChangesAsync();
        }

        public Task<int> CountAssignmentsForLicense(int licenseId) =>
            _db.Entitlements.CountAsync(e => e.LicenseId == licenseId);

    }
}
