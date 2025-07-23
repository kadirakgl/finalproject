using System;

namespace TaskManagement.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } // Örn: Create, Update, Delete
        public string EntityType { get; set; } // Örn: Task, Project, Issue
        public int EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }
} 