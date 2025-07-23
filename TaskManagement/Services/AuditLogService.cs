using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;
using System.Linq;

namespace TaskManagement.Services
{
    public class AuditLogService
    {
        private readonly AuditLogRepository _auditLogRepository;
        public AuditLogService(AuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }
        public List<Models.AuditLog> GetAllLogs()
        {
            return _auditLogRepository.GetAll();
        }
        public void AddLog(Models.AuditLog log)
        {
            _auditLogRepository.Add(log);
        }
        public List<Models.AuditLog> GetLogsByUserId(int userId)
        {
            return GetAllLogs().Where(l => l.UserId == userId).OrderByDescending(l => l.Timestamp).ToList();
        }
    }
} 