using System;

namespace TaskManagement.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; } // Açık, Kapalı, Atandı, vb.
        public string? Priority { get; set; } // Düşük, Orta, Yüksek
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? TaskId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? SolutionNote { get; set; }
    }
} 