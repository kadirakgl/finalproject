using TaskManagement.DataAccess;
using TaskManagement.Models;
using System;

namespace TaskManagement.Services
{
    public class SeedService
    {
        private readonly UserService _userService;
        private readonly ProjectService _projectService;
        private readonly TaskService _taskService;

        public SeedService(UserService userService, ProjectService projectService, TaskService taskService)
        {
            _userService = userService;
            _projectService = projectService;
            _taskService = taskService;
        }

        public void SeedDemoData()
        {
            // Eğer sistemde hiç kullanıcı yoksa seed işlemi yap
            if (_userService.GetAll().Count == 0 && _projectService.GetAll().Count == 0)
            {
                // Demo kullanıcılar
                _userService.Register("demo_manager", "1234", "Manager");
                _userService.Register("demo_user", "1234", "User");

                var manager = _userService.GetByUsername("demo_manager");
                var user = _userService.GetByUsername("demo_user");
                var admin = _userService.GetByUsername("admin");

                // Demo projeler
                var projectId = _projectService.AddProject("Demo Proje 1", "Açıklama 1", admin.Id);
                _taskService.AddTask(projectId, "Demo Görev 1", "Açıklama", "Yapılacak", user.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(3));
                _taskService.AddTask(projectId, "Demo Görev 2", "Açıklama", "Devam Ediyor", manager.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(5));
            }
        }
    }
} 