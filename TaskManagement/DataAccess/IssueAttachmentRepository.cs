using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;
using Dapper;
using System.Data;

namespace TaskManagement.DataAccess
{
    public class IssueAttachmentRepository
    {
        private readonly DapperContext _context;
        public IssueAttachmentRepository(DapperContext context)
        {
            _context = context;
        }
        public List<Models.IssueAttachment> GetByIssueId(int issueId)
        {
            using var connection = _context.CreateConnection();
            return connection.Query<Models.IssueAttachment>("SELECT * FROM IssueAttachments WHERE IssueId = @IssueId ORDER BY UploadedAt DESC", new { IssueId = issueId }).ToList();
        }
        public void Add(Models.IssueAttachment attachment)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO IssueAttachments (IssueId, FileName, FilePath, UploadedAt, UploadedByUserId)
                         VALUES (@IssueId, @FileName, @FilePath, @UploadedAt, @UploadedByUserId);";
            connection.Execute(sql, attachment);
        }
        public void Delete(int id)
        {
            using var connection = _context.CreateConnection();
            connection.Execute("DELETE FROM IssueAttachments WHERE Id = @Id", new { Id = id });
        }
        public void DeleteByUserId(int userId)
        {
            using var connection = _context.CreateConnection();
            connection.Execute("DELETE FROM IssueAttachments WHERE UploadedByUserId = @UserId", new { UserId = userId });
        }
    }
} 