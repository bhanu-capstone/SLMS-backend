using Microsoft.Extensions.Hosting;
using NotificationService.Clients;
using NotificationService.Repositories;

namespace NotificationService.BackgroundWorkers
{
    public class NotificationDispatcherWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationDispatcherWorker> _logger;

        public NotificationDispatcherWorker(IServiceProvider provider, IConfiguration cfg, ILogger<NotificationDispatcherWorker> logger)
        {
            _provider = provider;
            _config = cfg;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Dispatcher started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();

                    var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    var mail = scope.ServiceProvider.GetRequiredService<MailServiceClient>();

                    var pending = await repo.GetPendingAsync(50);
                    var due = await repo.GetDueScheduledAsync();

                    var all = pending.Concat(due).ToList();

                    foreach (var n in all)
                    {
                        bool ok = await mail.SendAsync(n.ToEmail, n.Subject, n.Body);

                        if (ok)
                            await repo.MarkAsSentAsync(n.Id);
                        else
                            await repo.MarkAsFailedAsync(n.Id, "Mail send failed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Dispatcher failed");
                }

                int pollSec = _config.GetValue<int>("Workers:DispatchPollSeconds", 5);
                await Task.Delay(TimeSpan.FromSeconds(pollSec), stoppingToken);
            }
        }
    }
}
