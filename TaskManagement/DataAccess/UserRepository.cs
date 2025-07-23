using System.Collections.Generic;
using System.Linq;
using Dapper;
using TaskManagement.Models;

namespace TaskManagement.DataAccess
{
    public class UserRepository
    {
        private readonly DapperContext _context;

        public UserRepository(DapperContext context)
        {
            _context = context;
        }

        public int Add(User user)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "INSERT INTO Users (Username, PasswordHash, Role, Email, Bio, IsApproved) VALUES (@Username, @PasswordHash, @Role, @Email, @Bio, @IsApproved); SELECT last_insert_rowid();";
                return connection.ExecuteScalar<int>(sql, user);
            }
        }

        public User GetById(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Users WHERE Id = @Id";
                return connection.QueryFirstOrDefault<User>(sql, new { Id = id });
            }
        }

        public User GetByUsername(string username)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Users WHERE Username = @Username";
                return connection.QueryFirstOrDefault<User>(sql, new { Username = username });
            }
        }

        public List<User> GetAll()
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Users";
                return connection.Query<User>(sql).ToList();
            }
        }

        public void Update(User user)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "UPDATE Users SET Username = @Username, PasswordHash = @PasswordHash, Role = @Role, Email = @Email, Bio = @Bio, LastActiveAt = @LastActiveAt, IsApproved = @IsApproved WHERE Id = @Id";
                connection.Execute(sql, user);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM Users WHERE Id = @Id";
                connection.Execute(sql, new { Id = id });
            }
        }
    }
}
