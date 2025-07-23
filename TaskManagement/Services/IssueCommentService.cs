using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;

namespace TaskManagement.Services
{
    public class IssueCommentService
    {
        private readonly IssueCommentRepository _commentRepository;
        public IssueCommentService(IssueCommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public List<Models.IssueComment> GetCommentsByIssueId(int issueId)
        {
            return _commentRepository.GetByIssueId(issueId);
        }
        public void AddComment(Models.IssueComment comment)
        {
            _commentRepository.Add(comment);
        }
        public void DeleteComment(int id)
        {
            _commentRepository.Delete(id);
        }
    }
} 