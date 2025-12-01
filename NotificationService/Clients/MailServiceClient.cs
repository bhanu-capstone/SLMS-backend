using System.Net.Http.Json;

namespace NotificationService.Clients
{
    public class MailServiceClient
    {
        private readonly HttpClient _http;

        public MailServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> SendAsync(string to, string subject, string body)
        {
            var payload = new
            {
                To = to,
                Subject = subject,
                Body = body
            };

            var resp = await _http.PostAsJsonAsync("/api/email/send", payload);
            return resp.IsSuccessStatusCode;
        }
    }
}
