using NotificationService.Models;

namespace NotificationService.Queues
{
    public interface IQueueProducer
    {
        Task PublishAsync(Notification notif);
    }
}
