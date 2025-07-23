using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;

namespace TaskManagement.Services
{
    public class AttachmentService
    {
        private readonly AttachmentRepository _attachmentRepository;
        public AttachmentService()
        {
            _attachmentRepository = new AttachmentRepository();
        }
        public List<Attachment> GetAttachmentsForTask(int taskId)
        {
            return _attachmentRepository.GetByTaskId(taskId);
        }
        public void AddAttachment(Attachment attachment)
        {
            _attachmentRepository.Add(attachment);
        }
    }
} 