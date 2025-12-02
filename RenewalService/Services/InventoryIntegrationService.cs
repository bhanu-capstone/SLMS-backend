using RenewalService.DTOs;

namespace RenewalService.Services
{
    public class InventoryIntegrationService
    {
        private readonly HttpClient _httpClient;

        public InventoryIntegrationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExternalLicenseDto?> GetLicenseDetailsAsync(int licenseId)
        {
            var response = await _httpClient.GetAsync($"api/Licenses/{licenseId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ExternalLicenseDto>();
        }

        public async Task<List<ExternalLicenseDto>> GetExpiringLicensesAsync(int days)
        {
            var response = await _httpClient.GetAsync($"api/Licenses/expiring/{days}");
            if (!response.IsSuccessStatusCode) return new List<ExternalLicenseDto>();

            var result = await response.Content.ReadFromJsonAsync<List<ExternalLicenseDto>>();
            return result ?? new List<ExternalLicenseDto>();
        }
    }
}