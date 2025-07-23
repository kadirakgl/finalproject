using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Services;
using System.Linq;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        public NotificationController()
        {
            _notificationService = new NotificationService();
        }
        public IActionResult Index()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId") ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var notifications = _notificationService.GetNotificationsForUser(userId);
            return View("Index", notifications);
        }
        public IActionResult Read(int id)
        {
            _notificationService.MarkAsRead(id);
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
} 