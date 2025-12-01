using System.Net.Http.Json;

namespace NotificationService.Clients
{
    public class ComplianceApiClient
    {
        private readonly HttpClient _http;

        public ComplianceApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task TriggerComplianceRunAsync()
        {
            var resp = await _http.PostAsync("/api/compliance/run", null);
            resp.EnsureSuccessStatusCode();
        }

        public record ComplianceEventDto(
            int Id,
            int LicenseId,
            string ProductName,
            string EventType,
            string Severity,
            string Details,
            DateTime CreatedAt,
            string? TimeLeft
        );

        public async Task<List<ComplianceEventDto>> GetEventsAsync()
        {
            return await _http.GetFromJsonAsync<List<ComplianceEventDto>>(
                "/api/compliance/alerts"
            ) ?? new List<ComplianceEventDto>();
        }
    }
}
