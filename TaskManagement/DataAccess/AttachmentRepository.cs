using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;

namespace TaskManagement.DataAccess
{
    public class AttachmentRepository
    {
        private static List<Attachment> _attachments = new List<Attachment>();
        private static int _nextId = 1;

        public List<Attachment> GetByTaskId(int taskId)
        {
            return _attachments.Where(a => a.TaskId == taskId).OrderByDescending(a => a.UploadedAt).ToList();
        }

        public void Add(Attachment attachment)
        {
            attachment.Id = _nextId++;
            _attachments.Add(attachment);
        }
    }
} 