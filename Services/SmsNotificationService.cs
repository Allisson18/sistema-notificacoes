using NotificationHub00.Entities;
using NotificationHub00.Interfaces;

namespace NotificationHub00.Services
{
    public class SmsNotificationService : INotificationService
    {
        public string Channel => "SMS";

        public Task<bool> SendNotificationsAsync(Notification notification)
        {
            // Simulação de envio corporativo de SMS (Mocada via Console)
            Console.WriteLine();
            return Task.FromResult(true);
        }
    }
}
