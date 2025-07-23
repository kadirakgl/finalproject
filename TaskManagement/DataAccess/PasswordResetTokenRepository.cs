using System;
using System.Linq;
using System.Collections.Generic;
using Dapper;
using TaskManagement.Models;

namespace TaskManagement.DataAccess
{
    public class PasswordResetTokenRepository
    {
        private readonly DapperContext _context;
        public PasswordResetTokenRepository()
        {
            _context = new DapperContext();
        }
        public void Add(PasswordResetToken token)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "INSERT INTO PasswordResetTokens (UserId, Token, Expiration) VALUES (@UserId, @Token, @Expiration);";
                connection.Execute(sql, token);
            }
        }
        public PasswordResetToken GetByToken(string token)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM PasswordResetTokens WHERE Token = @Token";
                return connection.QueryFirstOrDefault<PasswordResetToken>(sql, new { Token = token });
            }
        }
        public void DeleteByUserId(int userId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM PasswordResetTokens WHERE UserId = @UserId";
                connection.Execute(sql, new { UserId = userId });
            }
        }
    }
} 