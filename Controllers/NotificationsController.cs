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



        // Construtor
        public NotificationsController(ApplicationDbContext context, IEnumerable<INotificationService> notificationServices)
        {
            _context = context;
            _notificationServices = notificationServices;
        }


        // CRUD: Create (Envia e Salva)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationInputModel model)
        {
            var notification = new Notification(model.Target, model.Message, model.Channel);

            // Resolve dinamicamente o serviço correto usando Polimorfismo
            var service = _notificationServices.FirstOrDefault(s => s.Channel.Equals(model.Channel, StringComparison.OrdinalIgnoreCase));

            if (service == null)
            {
                return BadRequest("Canal de notificação não suportado.");
            }

            // Executa o envio mocado
            var sent = await service.SendNotificationsAsync(notification);

            if (!sent)
            {
                return StatusCode(500, "Erro ao processar o envio da notificação.");
            }

            // Salva o histórico no PostgreSQL
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
        }


        // CRUD: Read (Listar todas)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Notifications.OrderByDescending(n => n.CreatedAt).ToListAsync();
            return Ok(list);
        }

        // CRUD: Read (Buscar por ID)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(notification);

        }

        // DTO auxiliar para receber os dados do Frontend de forma segura

    }
}
