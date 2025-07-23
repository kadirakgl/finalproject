using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;

namespace TaskManagement.Services
{
    public class IssueAttachmentService
    {
        private readonly IssueAttachmentRepository _attachmentRepository;
        public IssueAttachmentService(IssueAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }
        public List<Models.IssueAttachment> GetAttachmentsByIssueId(int issueId)
        {
            return _attachmentRepository.GetByIssueId(issueId);
        }
        public void AddAttachment(Models.IssueAttachment attachment)
        {
            _attachmentRepository.Add(attachment);
        }
        public void DeleteAttachment(int id)
        {
            _attachmentRepository.Delete(id);
        }
    }
} 