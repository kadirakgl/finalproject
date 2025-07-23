using System;

namespace TaskManagement.Models
{
    public class IssueAttachment
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public int UploadedByUserId { get; set; }
    }
} 