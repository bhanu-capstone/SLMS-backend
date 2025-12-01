using System.Net.Http.Json;

namespace NotificationService.Clients
{
    public class AuthAdminClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<AuthAdminClient> _logger;

        public AuthAdminClient(HttpClient http, ILogger<AuthAdminClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public record AdminInfoDto(int UserId, string Email, string Username);

        /// <summary>
        /// Returns a list of admin emails (unique, non-empty).
        /// Falls back to a single fallback admin if call fails.
        /// </summary>
        public async Task<List<string>> GetAdminEmailsAsync(CancellationToken ct = default)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<AdminInfoDto>>("/api/user/it-emails", ct);
                if (res == null) return new List<string> { "bhanugit13@gmail.com" };

                var emails = res
                    .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                    .Select(x => x.Email.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!emails.Any())
                    return new List<string> { "bhanugit13@gmail.com" };

                return emails;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to fetch admin emails from AuthService; using fallback admin.\n"+ex.Message);
                return new List<string> { "bhanugit13@gmail.com" };
            }
        }
    }
}
