using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _db;

        public NotificationRepository(NotificationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Notification n)
        {
            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetPendingAsync(int limit = 50)
        {
            return await _db.Notifications
                .Where(n => n.Status == "pending" && n.DesiredSendAt == null)
                .OrderBy(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetDueScheduledAsync()
        {
            return await _db.Notifications
                .Where(n => n.Status == "pending" &&
                            n.DesiredSendAt != null &&
                            n.DesiredSendAt <= DateTime.UtcNow)
                .OrderBy(n => n.DesiredSendAt)
                .ToListAsync();
        }

        public async Task MarkAsSentAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null) return;
            n.Status = "sent";
            n.SentAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsFailedAsync(int id, string reason)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null) return;
            n.RetryCount++;
            n.Status = "failed";
            await _db.SaveChangesAsync();
        }
    }
}
