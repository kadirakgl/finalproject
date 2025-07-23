using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Services;

namespace TaskManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly AuditLogService _auditLogService;
        public AuditLogController(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }
        public IActionResult Index()
        {
            var logs = _auditLogService.GetAllLogs();
            return View(logs);
        }
    }
} 