using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;
using Dapper;
using System.Data;

namespace TaskManagement.DataAccess
{
    public class IssueCommentRepository
    {
        private readonly DapperContext _context;
        public IssueCommentRepository(DapperContext context)
        {
            _context = context;
        }
        public List<Models.IssueComment> GetByIssueId(int issueId)
        {
            using var connection = _context.CreateConnection();
            return connection.Query<Models.IssueComment>("SELECT * FROM IssueComments WHERE IssueId = @IssueId ORDER BY CreatedAt DESC", new { IssueId = issueId }).ToList();
        }
        public void Add(Models.IssueComment comment)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO IssueComments (IssueId, UserId, Content, CreatedAt)
                         VALUES (@IssueId, @UserId, @Content, @CreatedAt);";
            connection.Execute(sql, comment);
        }
        public void Delete(int id)
        {
            using var connection = _context.CreateConnection();
            connection.Execute("DELETE FROM IssueComments WHERE Id = @Id", new { Id = id });
        }
        public void DeleteByUserId(int userId)
        {
            using var connection = _context.CreateConnection();
            connection.Execute("DELETE FROM IssueComments WHERE UserId = @UserId", new { UserId = userId });
        }
    }
} 