using NotificationService.Models;

namespace NotificationService.Repositories
{
    public interface INotificationRepository
    {
        Task CreateAsync(Notification n);
        Task<List<Notification>> GetPendingAsync(int limit = 50);
        Task<List<Notification>> GetDueScheduledAsync();
        Task MarkAsSentAsync(int id);
        Task MarkAsFailedAsync(int id, string reason);
    }
}
