using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    public class LicenseRepository : ILicenseRepository
    {
        private readonly AppDbContext _db;
        public LicenseRepository(AppDbContext db) => _db = db;

        public Task<List<License>> GetAllAsync() =>
            _db.Licenses
               .Include(l => l.VendorContract)
               .Include(l => l.Entitlements)
               .Include(l => l.Installations)
               .ToListAsync();

        public Task<License?> GetByIdAsync(int id) =>
            _db.Licenses
               .Include(l => l.VendorContract)
               .Include(l => l.Entitlements)
               .Include(l => l.Installations)
               .FirstOrDefaultAsync(l => l.Id == id);

        public Task<License?> GetWithEntitlementsAsync(int id) =>
            _db.Licenses
               .Include(l => l.Entitlements)
               .ThenInclude(e => e.User)
               .Include(l => l.Entitlements)
               .ThenInclude(e => e.Device)
               .FirstOrDefaultAsync(l => l.Id == id);

        public Task<List<License>> SearchAsync(string? product, string? vendor)
        {
            return _db.Licenses
                .Where(l =>
                    (string.IsNullOrEmpty(product) || l.ProductName.Contains(product)) &&
                    (string.IsNullOrEmpty(vendor) || l.Vendor.Contains(vendor)))
                .ToListAsync();
        }

        public async Task AddAsync(License license)
        {
            await _db.Licenses.AddAsync(license);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(License license)
        {
            _db.Licenses.Update(license);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(License license)
        {
            _db.Licenses.Remove(license);
            await _db.SaveChangesAsync();
        }

        public async Task<List<License>> GetExpiringLicensesAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);
            var today = DateTime.UtcNow;

            // 2. Run the Query
            return await _db.Licenses
                .Where(l => l.ExpiryDate != null &&             // Must have an expiry date
                            l.ExpiryDate > today &&             // (Optional) Ignore items already expired
                            l.ExpiryDate <= cutoffDate)         // Expires before the cutoff
                .OrderBy(l => l.ExpiryDate)                     // Sort by most urgent first
                .ToListAsync();
        }
    }
}
