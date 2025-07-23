using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;
        private readonly UserService _userService;
        private readonly TaskService _taskService;
        private readonly NotificationService _notificationService;

        public ProjectController(ProjectService projectService, UserService userService, TaskService taskService)
        {
            _projectService = projectService;
            _userService = userService;
            _taskService = taskService;
            _notificationService = new NotificationService();
        }

        public IActionResult Index(string name = null, string description = null)
        {
            var projects = _projectService.GetAll();
            if (!string.IsNullOrEmpty(name))
                projects = projects.Where(p => p.Name != null && p.Name.Contains(name, System.StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(description))
                projects = projects.Where(p => p.Description != null && p.Description.Contains(description, System.StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.FilterName = name;
            ViewBag.FilterDescription = description;
            return View("Index", projects);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            var users = _userService.GetAll();
            ViewBag.Users = new SelectList(users, "Id", "Username");
            return View("Create");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _projectService.AddProject(project.Name, project.Description, project.OwnerId);
                // Proje sahibine bildirim gönder
                _notificationService.AddNotification(new Notification
                {
                    UserId = project.OwnerId,
                    Message = $"'{project.Name}' adlı proje size atandı.",
                    IsRead = false,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                });
                return RedirectToAction("Index");
            }
            var users = _userService.GetAll();
            ViewBag.Users = new SelectList(users, "Id", "Username");
            return View("Create", project);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var project = _projectService.GetById(id);
            var users = _userService.GetAll();
            ViewBag.Users = new SelectList(users, "Id", "Username");
            return View("Create", project);
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                var oldProject = _projectService.GetById(project.Id);
                _projectService.UpdateProject(project);
                // Eğer proje sahibi değiştiyse yeni sahibe bildirim gönder
                if (oldProject != null && oldProject.OwnerId != project.OwnerId)
                {
                    _notificationService.AddNotification(new Notification
                    {
                        UserId = project.OwnerId,
                        Message = $"'{project.Name}' adlı proje size atandı.",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                    });
                }
                return RedirectToAction("Index");
            }
            var users = _userService.GetAll();
            ViewBag.Users = new SelectList(users, "Id", "Username");
            return View("Create", project);
        }
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Delete(int id)
        {
            _projectService.DeleteProject(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var project = _projectService.GetById(id);
            if (project == null) return NotFound();
            var owner = _userService.GetById(project.OwnerId);
            string ownerName = owner != null ? owner.Username : "-";
            var projectTasks = _taskService.GetAll().Where(t => t.ProjectId == id).ToList();
            int completedCount = projectTasks.Count(t => t.Status == "Tamamlandı");
            int totalCount = projectTasks.Count;
            ViewBag.OwnerName = ownerName;
            ViewBag.ProjectTasks = projectTasks;
            ViewBag.CompletedCount = completedCount;
            ViewBag.TotalCount = totalCount;
            return View("Details", project);
        }

        [HttpPost]
        public IActionResult Complete(int id)
        {
            var project = _projectService.GetById(id);
            if (project == null) return NotFound();
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            if (project.OwnerId != userId)
                return Forbid();
            if (project.Status == "Tamamlandı")
                return RedirectToAction("Details", new { id });
            project.Status = "Tamamlandı";
            _projectService.UpdateProject(project);
            // Admin ve Manager'lara bildirim gönder
            var admins = _userService.GetAll().Where(u => u.Role == "Admin" || u.Role == "Manager").ToList();
            foreach (var admin in admins)
            {
                _notificationService.AddNotification(new Notification {
                    UserId = admin.Id,
                    Message = $"'{project.Name}' adlı proje {User.Identity.Name} tarafından tamamlandı.",
                    IsRead = false,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                });
            }
            return RedirectToAction("Details", new { id });
        }
    }
}