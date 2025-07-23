using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;

namespace TaskManagement.Services
{
    public class CommentService
    {
        private readonly CommentRepository _commentRepository;
        public CommentService()
        {
            _commentRepository = new CommentRepository();
        }
        public List<Comment> GetCommentsForTask(int taskId)
        {
            return _commentRepository.GetByTaskId(taskId);
        }
        public void AddComment(Comment comment)
        {
            _commentRepository.Add(comment);
        }
    }
} 