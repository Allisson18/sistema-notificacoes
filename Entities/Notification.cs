namespace NotificationHub00.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public string? Target { get; set; }
        public string? Message { get; set; }
        public string? Channel { get; set; }// "Email" ou "SMS"
        public DateTime CreatedAt { get; private set; }



        // Construtor exigido pelo EF Core
        protected Notification()
        {
            
        }

        // Construtor parmetrizado
        public Notification(string target, string message, string channel)
        {
            Id = Guid.NewGuid();
            Target = target;
            Message = message;
            Channel = channel;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
