using System;

namespace TaskManagement.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public int UploadedByUserId { get; set; }
    }
} 