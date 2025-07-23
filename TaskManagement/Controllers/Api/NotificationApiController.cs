using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Services;
using TaskManagement.Models;
using System.Linq;

namespace TaskManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationApiController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        public NotificationApiController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Kullanıcının tüm bildirimlerini getir
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId") ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var notifications = _notificationService.GetNotificationsForUser(userId);
            return Ok(notifications);
        }

        // Bildirimi okundu olarak işaretle
        [HttpPost("read/{id}")]
        [Authorize]
        public IActionResult MarkAsRead(int id)
        {
            _notificationService.MarkAsRead(id);
            return NoContent();
        }

        // Bildirimi sil
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // (Varsa) Silme işlemi eklenebilir
            return NoContent();
        }
    }
} 