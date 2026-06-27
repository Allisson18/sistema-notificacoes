using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationHub00.Data;
using NotificationHub00.DTOs;
using NotificationHub00.Entities;
using NotificationHub00.Interfaces;

namespace NotificationHub00.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<INotificationService> _notificationServices;

        public NotificationsController(
            ApplicationDbContext context,
            IEnumerable<INotificationService> notificationServices)
        {
            _context = context;
            _notificationServices = notificationServices;
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationInputModel model)
        {
            if (model == null)
                return BadRequest("Body não pode ser nulo.");

            if (string.IsNullOrWhiteSpace(model.Target) ||
                string.IsNullOrWhiteSpace(model.Message) ||
                string.IsNullOrWhiteSpace(model.Channel))
            {
                return BadRequest("Target, Message e Channel são obrigatórios.");
            }

            var notification = new Notification(model.Target, model.Message, model.Channel);

            var service = _notificationServices.FirstOrDefault(s =>
                !string.IsNullOrWhiteSpace(s.Channel) &&
                s.Channel.Equals(model.Channel, StringComparison.OrdinalIgnoreCase));

            if (service == null)
                return BadRequest("Canal de notificação não suportado.");

            var sent = await service.SendNotificationsAsync(notification);

            if (!sent)
                return StatusCode(500, "Erro ao processar envio da notificação.");

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
        }

        // =========================
        // READ ALL (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(list);
        }

        // =========================
        // READ BY ID (GET)
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound();

            return Ok(notification);
        }
    }
}