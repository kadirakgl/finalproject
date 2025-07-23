using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;
using Dapper;
using System.Data;
using System;

namespace TaskManagement.DataAccess
{
    public class IssueRepository
    {
        private readonly DapperContext _context;
        public IssueRepository(DapperContext context)
        {
            _context = context;
        }
        public List<Models.Issue> GetAll()
        {
            using var connection = _context.CreateConnection();
            return connection.Query<Models.Issue>("SELECT * FROM Issues ORDER BY CreatedAt DESC").ToList();
        }
        public Models.Issue? GetById(int id)
        {
            using var connection = _context.CreateConnection();
            return connection.QueryFirstOrDefault<Models.Issue>("SELECT * FROM Issues WHERE Id = @Id", new { Id = id });
        }
        public int Add(Models.Issue issue)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var sql = @"INSERT INTO Issues (Title, Description, Status, Priority, CreatedByUserId, AssignedToUserId, TaskId, CreatedAt, ClosedAt, SolutionNote)
                            VALUES (@Title, @Description, @Status, @Priority, @CreatedByUserId, @AssignedToUserId, @TaskId, @CreatedAt, @ClosedAt, @SolutionNote);
                            SELECT last_insert_rowid();";
                var id = connection.ExecuteScalar<int>(sql, issue);
                issue.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void Update(Models.Issue issue)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE Issues SET Title=@Title, Description=@Description, Status=@Status, Priority=@Priority, CreatedByUserId=@CreatedByUserId, AssignedToUserId=@AssignedToUserId, TaskId=@TaskId, CreatedAt=@CreatedAt, ClosedAt=@ClosedAt, SolutionNote=@SolutionNote WHERE Id=@Id";
            connection.Execute(sql, issue);
        }
        public void Delete(int id)
        {
            using var connection = _context.CreateConnection();
            connection.Execute("DELETE FROM Issues WHERE Id = @Id", new { Id = id });
        }
        public void DeleteByUserId(int userId)
        {
            using var connection = _context.CreateConnection();
            connection.Execute("DELETE FROM Issues WHERE CreatedByUserId = @UserId OR AssignedToUserId = @UserId", new { UserId = userId });
        }
    }
} 