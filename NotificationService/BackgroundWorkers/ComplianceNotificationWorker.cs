using Microsoft.Extensions.Hosting;
using NotificationService.Clients;
using NotificationService.Repositories;
using NotificationService.Models;

namespace NotificationService.BackgroundWorkers
{
    public class ComplianceNotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;
        private readonly ILogger<ComplianceNotificationWorker> _logger;

        public ComplianceNotificationWorker(
            IServiceProvider provider,
            IConfiguration cfg,
            ILogger<ComplianceNotificationWorker> logger)
        {
            _provider = provider;
            _config = cfg;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔵 Compliance Notification Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();

                    var complianceClient = scope.ServiceProvider.GetRequiredService<ComplianceApiClient>();
                    var authClient = scope.ServiceProvider.GetRequiredService<AuthAdminClient>();
                    var mail = scope.ServiceProvider.GetRequiredService<MailServiceClient>(); // direct sending
                    var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    // Step 1: Trigger compliance engine run
                    _logger.LogInformation("🚀 Triggering compliance engine run...");
                    await complianceClient.TriggerComplianceRunAsync();

                    // Step 2: Get alerts
                    _logger.LogInformation("📡 Fetching compliance alerts...");
                    var alerts = await complianceClient.GetEventsAsync();

                    _logger.LogInformation("📥 Retrieved {count} alerts.", alerts.Count);

                    if (alerts.Count > 0)
                    {
                        // Step 3: get admin emails
                        var admins = await authClient.GetAdminEmailsAsync(stoppingToken);
                        _logger.LogInformation("🔑 IT Admin emails: {emails}", string.Join(", ", admins));

                        // Step 4: Send email for each alert
                        foreach (var alert in alerts)
                        {
                            var body = new System.Text.StringBuilder();
                            body.AppendLine($"Event Type: {alert.EventType}");
                            body.AppendLine($"Product: {alert.ProductName}");
                            body.AppendLine($"Severity: {alert.Severity}");
                            body.AppendLine($"Details: {alert.Details}");
                            body.AppendLine($"Recorded At: {alert.CreatedAt}");
                            body.AppendLine("This is an automated compliance notification.");

                            foreach (var adminEmail in admins)
                            {
                                var subject = $"[COMPLIANCE] {alert.EventType.ToUpper()} — {alert.ProductName}";

                                // send immediately
                                bool sent = await mail.SendAsync(adminEmail, subject, body.ToString());

                                // DB log (optional)
                                var notif = new Notification
                                {
                                    ToEmail = adminEmail,
                                    Subject = subject,
                                    Body = body.ToString(),
                                    Type = $"compliance_{alert.EventType}",
                                    Channel = "email",
                                    Status = sent ? "sent" : "failed",
                                    SentAt = sent ? DateTime.UtcNow : null,
                                    DesiredSendAt = DateTime.UtcNow
                                };

                                await repo.CreateAsync(notif);

                                _logger.LogInformation("📨 Notification sent={sent} admin={admin} eventId={id}",
                                    sent, adminEmail, alert.Id);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No alerts found this cycle.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Compliance Notification Worker failed");
                }

                var sleep = _config.GetValue<int>("Workers:CompliancePollSeconds", 30);
                await Task.Delay(TimeSpan.FromSeconds(sleep), stoppingToken);
            }
        }
    }
}
