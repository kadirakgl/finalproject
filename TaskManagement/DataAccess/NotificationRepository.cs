using System.Collections.Generic;
using System.Linq;
using TaskManagement.Models;
using System;
using Dapper;
using System.Data;

namespace TaskManagement.DataAccess
{
    public class NotificationRepository
    {
        private readonly DapperContext _context;
        public NotificationRepository()
        {
            _context = new DapperContext();
        }
        public List<Notification> GetByUserId(int userId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "SELECT * FROM Notifications WHERE UserId = @UserId ORDER BY CreatedAt DESC";
                return connection.Query<Notification>(sql, new { UserId = userId }).ToList();
            }
        }
        public void Add(Notification notification)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "INSERT INTO Notifications (UserId, Message, IsRead, CreatedAt) VALUES (@UserId, @Message, @IsRead, @CreatedAt);";
                connection.Execute(sql, new { notification.UserId, notification.Message, IsRead = notification.IsRead ? 1 : 0, notification.CreatedAt });
            }
        }
        public void MarkAsRead(int notificationId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "UPDATE Notifications SET IsRead = 1 WHERE Id = @Id";
                connection.Execute(sql, new { Id = notificationId });
            }
        }
        public void DeleteByUserId(int userId)
        {
            using (var connection = _context.CreateConnection())
            {
                string sql = "DELETE FROM Notifications WHERE UserId = @UserId";
                connection.Execute(sql, new { UserId = userId });
            }
        }
    }
} 