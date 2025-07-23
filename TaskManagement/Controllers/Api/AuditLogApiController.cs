using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Services;

namespace TaskManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogApiController : ControllerBase
    {
        private readonly AuditLogService _auditLogService;
        public AuditLogApiController(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // Tüm logları getir (sadece admin)
        [HttpGet]
        public IActionResult GetAll()
        {
            var logs = _auditLogService.GetAllLogs();
            return Ok(logs);
        }
    }
} 