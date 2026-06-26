using NotificationHub00.Entities;
using NotificationHub00.Interfaces;

namespace NotificationHub00.Services
{
    public class EmailNotificationService : INotificationService
    {
        public string Channel => "Email";

        public Task<bool> SendNotificationsAsync(Notification notification)
        {
            // Simulação de envio corporativo de e-mail (Mocada via Console)
            Console.WriteLine($"[EMAIL ENVIADO] Para: {notification.Target} | Mensagem: {notification.Message}");
            return Task.FromResult(true);
        }
    }
}
