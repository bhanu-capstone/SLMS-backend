using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Queues
{
    public class DbQueueProducer : IQueueProducer
    {
        private readonly INotificationRepository _repo;

        public DbQueueProducer(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task PublishAsync(Notification notif)
        {
            notif.Status = "pending";
            notif.CreatedAt = DateTime.UtcNow;

            await _repo.CreateAsync(notif);
        }
    }
}
