using System.Collections.Generic;
using System.Linq;
using Dapper;
using TaskManagement.Models;

namespace TaskManagement.DataAccess
{
    public class ProjectRepository
    {
        private readonly DapperContext _context;

        public ProjectRepository(DapperContext context)
        {
            _context = context;
        }

        public int Add(Project project)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "INSERT INTO Projects (Name, Description, OwnerId, Status) VALUES (@Name, @Description, @OwnerId, @Status); SELECT last_insert_rowid();";
                return connection.ExecuteScalar<int>(sql, project);
            }
        }

        public Project GetById(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Projects WHERE Id = @Id";
                return connection.QueryFirstOrDefault<Project>(sql, new { Id = id });
            }
        }

        public List<Project> GetAll()
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Projects";
                return connection.Query<Project>(sql).ToList();
            }
        }

        public List<Project> GetByOwnerId(int ownerId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Projects WHERE OwnerId = @OwnerId";
                return connection.Query<Project>(sql, new { OwnerId = ownerId }).ToList();
            }
        }

        public void Update(Project project)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "UPDATE Projects SET Name = @Name, Description = @Description, OwnerId = @OwnerId, Status = @Status WHERE Id = @Id";
                connection.Execute(sql, project);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM Projects WHERE Id = @Id";
                connection.Execute(sql, new { Id = id });
            }
        }

        public void DeleteByOwnerId(int ownerId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM Projects WHERE OwnerId = @OwnerId";
                connection.Execute(sql, new { OwnerId = ownerId });
            }
        }
    }
}
