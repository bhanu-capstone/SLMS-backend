using System.Net.Http.Json;
using ComplianceService.Models;

namespace ComplianceService.Services
{
    public class InventoryApiClient
    {
        private readonly HttpClient _http;

        public InventoryApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<LicenseModel>> GetLicensesAsync()
        {
            return await _http.GetFromJsonAsync<List<LicenseModel>>("/api/licenses")
                   ?? new List<LicenseModel>();
        }

        public async Task<LicenseModel> GetLicenseById(int licenseId)
        {
            return await _http.GetFromJsonAsync<LicenseModel>($"/api/licenses/{licenseId}")
                   ?? new LicenseModel();
        }

        public async Task<List<InstalledSoftwareModel>> GetInstalledAsync()
        {
            return await _http.GetFromJsonAsync<List<InstalledSoftwareModel>>("/api/installations")
                   ?? new List<InstalledSoftwareModel>();
        }

        public async Task<List<EntitlementModel>> GetEntitlementsAsync()
        {
            return await _http.GetFromJsonAsync<List<EntitlementModel>>("/api/entitlements")
                   ?? new List<EntitlementModel>();
        }
    }
}
