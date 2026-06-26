using NotificationHub00.Entities;

namespace NotificationHub00.Interfaces
{
    public interface INotificationService
    {
        string Channel { get; }
        Task<bool> SendNotificationsAsync(Notification notification);
    }
}
