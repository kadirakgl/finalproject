using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;

namespace TaskManagement.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _notificationRepository;
        public NotificationService()
        {
            _notificationRepository = new NotificationRepository();
        }
        public List<Notification> GetNotificationsForUser(int userId)
        {
            return _notificationRepository.GetByUserId(userId);
        }
        public void AddNotification(Notification notification)
        {
            _notificationRepository.Add(notification);
        }
        public void MarkAsRead(int notificationId)
        {
            _notificationRepository.MarkAsRead(notificationId);
        }
    }
} 