using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;
using Dapper;
using System.Data;

namespace TaskManagement.DataAccess
{
    public class AuditLogRepository
    {
        private readonly DapperContext _context;
        public AuditLogRepository(DapperContext context)
        {
            _context = context;
        }
        public List<Models.AuditLog> GetAll()
        {
            using var connection = _context.CreateConnection();
            return connection.Query<Models.AuditLog>("SELECT * FROM AuditLogs ORDER BY Timestamp DESC").ToList();
        }
        public void Add(Models.AuditLog log)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, Timestamp, Description)
                         VALUES (@UserId, @Action, @EntityType, @EntityId, @Timestamp, @Description);";
            connection.Execute(sql, log);
        }
    }
} 