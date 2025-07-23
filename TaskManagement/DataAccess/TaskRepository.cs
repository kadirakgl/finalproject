using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TaskManagement.Models;
using Task = TaskManagement.Models.Task;

namespace TaskManagement.DataAccess
{
    public class TaskRepository
    {
        private readonly DapperContext _context;

        public TaskRepository(DapperContext context)
        {
            _context = context;
        }

        public int Add(Task task)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "INSERT INTO Tasks (ProjectId, Title, Description, Status, AssignedUserId, CreatedAt, DueDate) VALUES (@ProjectId, @Title, @Description, @Status, @AssignedUserId, @CreatedAt, @DueDate); SELECT last_insert_rowid();";
                return connection.ExecuteScalar<int>(sql, task);
            }
        }

        public Task GetById(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Tasks WHERE Id = @Id";
                return connection.QueryFirstOrDefault<Task>(sql, new { Id = id });
            }
        }

        public List<Task> GetAll()
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Tasks";
                return connection.Query<Task>(sql).ToList();
            }
        }

        public List<Task> GetByProjectId(int projectId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Tasks WHERE ProjectId = @ProjectId";
                return connection.Query<Task>(sql, new { ProjectId = projectId }).ToList();
            }
        }

        public List<Models.Task> GetByAssignedUserId(int userId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Tasks WHERE AssignedUserId = @UserId";
                return connection.Query<Models.Task>(sql, new { UserId = userId }).ToList();
            }
        }

        public void Update(Task task)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "UPDATE Tasks SET ProjectId = @ProjectId, Title = @Title, Description = @Description, Status = @Status, AssignedUserId = @AssignedUserId, CreatedAt = @CreatedAt, DueDate = @DueDate WHERE Id = @Id";
                connection.Execute(sql, task);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM Tasks WHERE Id = @Id";
                connection.Execute(sql, new { Id = id });
            }
        }

        public void DeleteByAssignedUserId(int userId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM Tasks WHERE AssignedUserId = @UserId";
                connection.Execute(sql, new { UserId = userId });
            }
        }
    }
}
