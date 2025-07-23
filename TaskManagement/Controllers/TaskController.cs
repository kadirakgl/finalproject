using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;
using System;
using Task = TaskManagement.Models.Task;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using TaskManagement.Models;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly TaskService _taskService;
        private readonly ProjectService _projectService;
        private readonly UserService _userService;
        private readonly CommentService _commentService;
        private readonly AttachmentService _attachmentService;
        private readonly NotificationService _notificationService;

        public TaskController(TaskService taskService, ProjectService projectService, UserService userService)
        {
            _taskService = taskService;
            _projectService = projectService;
            _userService = userService;
            _commentService = new CommentService();
            _attachmentService = new AttachmentService();
            _notificationService = new NotificationService();
        }

        public IActionResult Index(string title = null, string status = null, int? projectId = null, int? assignedUserId = null)
        {
            var isAdmin = User.IsInRole("Admin");
            var tasks = isAdmin ? _taskService.GetAll() : _taskService.GetByAssignedUserId(GetCurrentUserId());

            if (!string.IsNullOrEmpty(title))
                tasks = tasks.Where(t => t.Title != null && t.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(status))
                tasks = tasks.Where(t => t.Status == status).ToList();
            if (projectId.HasValue)
                tasks = tasks.Where(t => t.ProjectId == projectId.Value).ToList();
            if (assignedUserId.HasValue)
                tasks = tasks.Where(t => t.AssignedUserId == assignedUserId.Value).ToList();

            ViewBag.Projects = new SelectList(_projectService.GetAll(), "Id", "Name");
            ViewBag.Users = new SelectList(_userService.GetAll(), "Id", "Username");
            ViewBag.StatusList = new SelectList(new[] { "Yapılacak", "Devam Ediyor", "Tamamlandı" });
            ViewBag.IsAdmin = isAdmin;
            ViewBag.FilterTitle = title;
            ViewBag.FilterStatus = status;
            ViewBag.FilterProjectId = projectId?.ToString();
            ViewBag.FilterAssignedUserId = assignedUserId?.ToString();
            return View("Index", tasks);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            var projects = _projectService.GetAll();
            var users = _userService.GetAll();
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            ViewBag.Users = new SelectList(users, "Id", "Username");
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View("Create");
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult Create(Task task)
        {
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                // User ise, AssignedUserId'yi giriş yapan kullanıcıya ayarla
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                task.AssignedUserId = userId;
            }
            if (ModelState.IsValid)
            {
                _taskService.AddTask(task.ProjectId, task.Title, task.Description, task.Status, task.AssignedUserId ?? 0, DateTime.UtcNow, task.DueDate);
                if (task.AssignedUserId.HasValue)
                {
                    _notificationService.AddNotification(new Notification {
                        UserId = task.AssignedUserId.Value,
                        Message = $"'{task.Title}' adlı görev size atandı.",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                    });
                }
                return RedirectToAction("Index");
            }
            var projects = _projectService.GetAll();
            var users = _userService.GetAll();
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            ViewBag.Users = new SelectList(users, "Id", "Username");
            ViewBag.IsAdmin = isAdmin;
            return View("Create", task);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Edit(int id)
        {
            var task = _taskService.GetById(id);
            var projects = _projectService.GetAll();
            var users = _userService.GetAll();
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            ViewBag.Users = new SelectList(users, "Id", "Username");
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View("Create", task);
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult Edit(Task task)
        {
            if (ModelState.IsValid)
            {
                var oldTask = _taskService.GetById(task.Id);
                if (oldTask != null)
                {
                    task.CreatedAt = oldTask.CreatedAt; // Eski oluşturulma tarihini koru
                }
                _taskService.UpdateTask(task);
                if (task.AssignedUserId.HasValue)
                {
                    _notificationService.AddNotification(new Notification {
                        UserId = task.AssignedUserId.Value,
                        Message = $"'{task.Title}' adlı göreviniz güncellendi.",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                    });
                }
                return RedirectToAction("Index");
            }
            var projects = _projectService.GetAll();
            var users = _userService.GetAll();
            ViewBag.Projects = new SelectList(projects, "Id", "Name");
            ViewBag.Users = new SelectList(users, "Id", "Username");
            return View("Create", task);
        }
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Delete(int id)
        {
            _taskService.DeleteTask(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult StatusUpdate(int id, string status)
        {
            var task = _taskService.GetById(id);
            if (task == null)
                return NotFound();

            var isAdmin = User.IsInRole("Admin");
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

            // Sadece admin veya kendisine atanmış görev için izin ver
            if (isAdmin || (task.AssignedUserId == userId))
            {
                task.Status = status;
                _taskService.UpdateTask(task);
                if (task.AssignedUserId.HasValue)
                {
                    _notificationService.AddNotification(new Notification {
                        UserId = task.AssignedUserId.Value,
                        Message = $"'{task.Title}' adlı görevinizin durumu '{status}' olarak değiştirildi.",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                    });
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Complete(int id)
        {
            var task = _taskService.GetById(id);
            if (task == null) return NotFound();
            var userId = GetCurrentUserId();
            if (task.AssignedUserId != userId)
                return Forbid();
            if (task.Status == "Tamamlandı")
                return RedirectToAction("Details", new { id });
            task.Status = "Tamamlandı";
            _taskService.UpdateTask(task);
            // Admin ve Manager'lara bildirim gönder
            var admins = _userService.GetAll().Where(u => u.Role == "Admin" || u.Role == "Manager").ToList();
            foreach (var admin in admins)
            {
                _notificationService.AddNotification(new Notification {
                    UserId = admin.Id,
                    Message = $"'{task.Title}' adlı görev {User.Identity.Name} tarafından tamamlandı.",
                    IsRead = false,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                });
            }
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var task = _taskService.GetById(id);
            if (task == null) return NotFound();
            var comments = _commentService.GetCommentsForTask(id);
            var attachments = _attachmentService.GetAttachmentsForTask(id);
            var project = _projectService.GetById(task.ProjectId);
            string projectName = project != null ? project.Name : "-";
            string assignedUserName = "-";
            if (task.AssignedUserId.HasValue)
            {
                var user = _userService.GetById(task.AssignedUserId.Value);
                assignedUserName = user != null ? user.Username : "-";
            }
            // İlgili ticketlar (issues)
            var issueService = new TaskManagement.Services.IssueService(new TaskManagement.DataAccess.IssueRepository(new TaskManagement.DataAccess.DapperContext()));
            var relatedIssues = issueService.GetAll().Where(i => i.TaskId == id).ToList();
            ViewBag.Comments = comments;
            ViewBag.Attachments = attachments;
            ViewBag.ProjectName = projectName;
            ViewBag.AssignedUserName = assignedUserName;
            ViewBag.RelatedIssues = relatedIssues;
            return View("Details", task);
        }

        [HttpPost]
        public IActionResult AddComment(int taskId, string content)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            if (!string.IsNullOrWhiteSpace(content))
            {
                _commentService.AddComment(new Comment {
                    TaskId = taskId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                });
                // Adminlere bildirim gönder
                var admins = _userService.GetAll().Where(u => u.Role == "Admin").ToList();
                var task = _taskService.GetById(taskId);
                foreach (var admin in admins)
                {
                    _notificationService.AddNotification(new Notification {
                        UserId = admin.Id,
                        Message = $"'{task.Title}' adlı göreve yeni bir yorum eklendi.",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                    });
                }
            }
            return RedirectToAction("Details", new { id = taskId });
        }

        [HttpPost]
        public IActionResult AddAttachment(int taskId)
        {
            var files = Request.Form.Files;
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var uploaded = new List<Attachment>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    var attachment = new Attachment {
                        TaskId = taskId,
                        FileName = fileName,
                        FilePath = "/uploads/" + fileName,
                        UploadedAt = DateTime.UtcNow,
                        UploadedByUserId = userId
                    };
                    _attachmentService.AddAttachment(attachment);
                    uploaded.Add(attachment);
                }
            }
            // AJAX isteği ise JSON dön
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("multipart/form-data") == true)
            {
                return Json(new {
                    success = true,
                    attachments = uploaded.Select(a => new {
                        fileName = a.FileName,
                        filePath = a.FilePath,
                        uploadedAt = a.UploadedAt.ToString("g"),
                        uploadedByUserId = a.UploadedByUserId
                    }).ToList()
                });
            }
            // Normal form submit ise redirect
            return RedirectToAction("Details", new { id = taskId });
        }

        [HttpGet]
        public IActionResult Calendar()
        {
            var tasks = _taskService.GetAll();
            var calendarEvents = tasks.Select(t => new {
                id = t.Id,
                title = t.Title,
                start = t.DueDate?.ToString("yyyy-MM-dd"),
                url = Url.Action("Details", new { id = t.Id })
            }).ToList();
            ViewBag.CalendarEvents = System.Text.Json.JsonSerializer.Serialize(calendarEvents);
            return View("Calendar");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Kanban()
        {
            var tasks = _taskService.GetAll();
            return View(tasks);
        }
    }
}
