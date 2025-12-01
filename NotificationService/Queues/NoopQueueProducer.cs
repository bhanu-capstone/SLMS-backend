using NotificationService.Models;

namespace NotificationService.Queues
{
    public class NoopQueueProducer : IQueueProducer
    {
        public Task PublishAsync(Notification notif)
        {
            return Task.CompletedTask;
        }
    }
}
