using TaskManagement.DataAccess;
using TaskManagement.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TaskManagement.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly IssueRepository _issueRepository;
        private readonly IssueCommentRepository _issueCommentRepository;
        private readonly IssueAttachmentRepository _issueAttachmentRepository;
        private readonly TaskRepository _taskRepository;
        private readonly ProjectRepository _projectRepository;

        public UserService(
            UserRepository userRepository,
            NotificationRepository notificationRepository,
            IssueRepository issueRepository,
            IssueCommentRepository issueCommentRepository,
            IssueAttachmentRepository issueAttachmentRepository,
            TaskRepository taskRepository,
            ProjectRepository projectRepository)
        {
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _issueRepository = issueRepository;
            _issueCommentRepository = issueCommentRepository;
            _issueAttachmentRepository = issueAttachmentRepository;
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                // Hexadecimal string olarak döndür
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public int Register(string username, string password, string role, string email = null)
        {
            string passwordHash = HashPassword(password);
            var user = new User { Username = username, PasswordHash = passwordHash, Role = role, Email = email, IsApproved = false };
            return _userRepository.Add(user);
        }

        public User GetById(int id)
        {
            return _userRepository.GetById(id);
        }

        public User GetByUsername(string username)
        {
            return _userRepository.GetByUsername(username);
        }

        public List<User> GetAll() => _userRepository.GetAll();
        public int AddUser(User user) => _userRepository.Add(user);
        public void UpdateUser(User user) => _userRepository.Update(user);
        public void DeleteUser(int id) => _userRepository.Delete(id);
        public void ApproveUser(int userId)
        {
            var user = _userRepository.GetById(userId);
            if (user != null)
            {
                user.IsApproved = true;
                _userRepository.Update(user);
            }
        }
        public void RejectUser(int userId)
        {
            _userRepository.Delete(userId);
        }
    }
}
