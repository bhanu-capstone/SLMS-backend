using InventoryService.Models;

namespace InventoryService.Repositories
{
    public interface ILicenseRepository
    {
        Task<List<License>> GetAllAsync();
        Task<License?> GetByIdAsync(int id);
        Task<List<License>> SearchAsync(string? product, string? vendor);
        Task AddAsync(License license);
        Task UpdateAsync(License license);
        Task<List<License>> GetExpiringLicensesAsync(int days);
        Task DeleteAsync(License license);

        // Used by EntitlementService
        Task<License?> GetWithEntitlementsAsync(int id);
    }
}
