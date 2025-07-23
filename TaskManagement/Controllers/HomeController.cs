using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TaskService _taskService;
    private readonly ProjectService _projectService;
    private readonly UserService _userService;
    private readonly NotificationService _notificationService;
    private readonly EmailService _emailService;

    public HomeController(ILogger<HomeController> logger, TaskService taskService, ProjectService projectService, UserService userService, NotificationService notificationService, EmailService emailService)
    {
        _logger = logger;
        _taskService = taskService;
        _projectService = projectService;
        _userService = userService;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return View("Welcome");
        }

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        var userIdClaim = User.FindFirst("UserId");
        int currentUserId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;

        var tasks = isAdmin ? _taskService.GetAll() : _taskService.GetByAssignedUserId(currentUserId);
        var projects = isAdmin ? _projectService.GetAll() : _projectService.GetByOwnerId(currentUserId);
        var users = _userService.GetAll();
        var completedTasks = tasks.Count(t => t.Status == "Tamamlandı");
        ViewBag.TaskCount = tasks.Count;
        ViewBag.ProjectCount = projects.Count;
        ViewBag.UserCount = users.Count;
        ViewBag.CompletedTaskCount = completedTasks;
        // Görev durum dağılımı
        var statusGroups = tasks.GroupBy(t => t.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
        ViewBag.StatusChartLabels = System.Text.Json.JsonSerializer.Serialize(statusGroups.Select(g => g.Status));
        ViewBag.StatusChartData = System.Text.Json.JsonSerializer.Serialize(statusGroups.Select(g => g.Count));
        // Görev dağılımı tablosu için detaylı liste
        var taskList = tasks.Select(t => new {
            t.Title,
            AssignedUser = users.FirstOrDefault(u => u.Id == t.AssignedUserId)?.Username ?? "-",
            AssignedDate = t.CreatedAt,
            t.Status
        }).ToList();
        ViewBag.TaskList = taskList;
        var projectStatusGroups = projects.GroupBy(p => p.Status ?? "Bilinmiyor").Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
        ViewBag.ProjectStatusChartLabels = System.Text.Json.JsonSerializer.Serialize(projectStatusGroups.Select(g => g.Status));
        ViewBag.ProjectStatusChartData = System.Text.Json.JsonSerializer.Serialize(projectStatusGroups.Select(g => g.Count));

        // Ticket (Issue) istatistikleri
        var issueService = HttpContext.RequestServices.GetService(typeof(TaskManagement.Services.IssueService)) as TaskManagement.Services.IssueService;
        var issues = isAdmin ? issueService.GetAll() : issueService.GetAll().Where(i => i.AssignedToUserId == currentUserId || i.CreatedByUserId == currentUserId).ToList();
        ViewBag.IssueCount = issues.Count;
        ViewBag.OpenIssueCount = issues.Count(i => i.Status == "Açık");
        ViewBag.ClosedIssueCount = issues.Count(i => i.Status == "Kapalı");
        var issueStatusGroups = issues.GroupBy(i => i.Status ?? "Bilinmiyor").Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
        ViewBag.IssueStatusChartLabels = System.Text.Json.JsonSerializer.Serialize(issueStatusGroups.Select(g => g.Status));
        ViewBag.IssueStatusChartData = System.Text.Json.JsonSerializer.Serialize(issueStatusGroups.Select(g => g.Count));
        ViewBag.RecentIssues = issues.OrderByDescending(i => i.CreatedAt).Take(5).ToList();
        // Bildirimler
        var allNotifications = _notificationService.GetNotificationsForUser(currentUserId)?.OrderByDescending(n => n.CreatedAt).Take(10).ToList() ?? new List<TaskManagement.Models.Notification>();
        ViewBag.Notifications = allNotifications;
        ViewBag.UnreadNotificationCount = allNotifications.Count(n => !n.IsRead);
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Help()
    {
        return View("Help");
    }

    [HttpPost]
    public async Task<IActionResult> Contact(string ad, string email, string mesaj)
    {
        var html = $"<b>Sayın {ad},</b><br>Mesajınız başarıyla alınmıştır.<br>En kısa sürede sizinle iletişime geçeceğiz.<br><br><b>Mesajınız:</b><br>{mesaj}";
        var result = await _emailService.SendEmailAsync(
            to: email,
            subject: "İletişim Formu Mesajınız Alındı",
            htmlContent: html,
            from: "kadirak.2001@hotmail.com",
            fromName: "TaskManager Admin"
        );
        TempData["ContactResult"] = result ? "Mesajınız başarıyla gönderildi!" : "Mesaj gönderilemedi. Lütfen tekrar deneyin.";
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
