using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;

namespace TaskManagement.DataAccess
{
    public class CommentRepository
    {
        private static List<Comment> _comments = new List<Comment>();
        private static int _nextId = 1;

        public List<Comment> GetByTaskId(int taskId)
        {
            return _comments.Where(c => c.TaskId == taskId).OrderByDescending(c => c.CreatedAt).ToList();
        }

        public void Add(Comment comment)
        {
            comment.Id = _nextId++;
            _comments.Add(comment);
        }
    }
} 