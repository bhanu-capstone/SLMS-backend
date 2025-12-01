using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> opts) : base(opts) { }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(b =>
            {
                b.HasKey(n => n.Id);
                b.Property(n => n.ToEmail).IsRequired();
                b.Property(n => n.Subject).IsRequired();
                b.Property(n => n.Body).IsRequired();
                b.Property(n => n.Status).HasDefaultValue("pending");
                b.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
