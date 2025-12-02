using Microsoft.EntityFrameworkCore;
using RenewalService.Data;
using RenewalService.Models;

namespace RenewalService.Repositories
{
    public class RenewalRepository
    {
        private readonly RenewalDbContext _context;

        public RenewalRepository(RenewalDbContext context)
        {
            _context = context;
        }

        public Task<List<RenewalRecord>> GetAllAsync() =>
            _context.Renewals.ToListAsync();

        public Task<RenewalRecord?> GetByIdAsync(int id) =>
            _context.Renewals.FindAsync(id).AsTask();

        public Task<RenewalRecord?> GetByLicenseIdAsync(int licenseId) =>
            _context.Renewals.FirstOrDefaultAsync(r => r.LinkedLicenseId == licenseId);

        public async Task AddAsync(RenewalRecord record)
        {
            await _context.Renewals.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RenewalRecord record)
        {
            _context.Renewals.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var record = await GetByIdAsync(id);
            if (record != null)
            {
                _context.Renewals.Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}