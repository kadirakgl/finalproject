using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Services;
using TaskManagement.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class IssueController : Controller
    {
        private readonly IssueService _issueService;
        private readonly UserService _userService;
        private readonly AuditLogService _auditLogService;
        private readonly IssueCommentService _commentService;
        private readonly IssueAttachmentService _attachmentService;
        private readonly NotificationService _notificationService;
        private readonly EmailService _emailService;
        
        public IssueController(
            IssueService issueService,
            UserService userService,
            AuditLogService auditLogService,
            IssueCommentService commentService,
            IssueAttachmentService attachmentService,
            NotificationService notificationService,
            EmailService emailService)
        {
            _issueService = issueService;
            _userService = userService;
            _auditLogService = auditLogService;
            _commentService = commentService;
            _attachmentService = attachmentService;
            _notificationService = notificationService;
            _emailService = emailService;
        }
        public IActionResult Index(string title = null, string status = null, string priority = null, int? createdByUserId = null)
        {
            var issues = _issueService.GetAll();
            if (!string.IsNullOrEmpty(title))
                issues = issues.Where(i => i.Title != null && i.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(status))
                issues = issues.Where(i => i.Status == status).ToList();
            if (!string.IsNullOrEmpty(priority))
                issues = issues.Where(i => i.Priority == priority).ToList();
            if (createdByUserId.HasValue)
                issues = issues.Where(i => i.CreatedByUserId == createdByUserId.Value).ToList();
            var users = _userService.GetAll();
            ViewBag.UserDict = users.ToDictionary(u => u.Id, u => u.Username);
            ViewBag.Users = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(users, "Id", "Username");
            ViewBag.FilterTitle = title;
            ViewBag.FilterStatus = status;
            ViewBag.FilterPriority = priority;
            ViewBag.FilterCreatedByUserId = createdByUserId?.ToString();
            ViewBag.StatusList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Açık", "Kapalı" });
            ViewBag.PriorityList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(new[] { "Düşük", "Orta", "Yüksek" });
            return View("Index", issues);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }
        [HttpPost]
        public IActionResult Create(Issue issue)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        // Console.WriteLine($"[MODELSTATE ERROR] {key}: {error.ErrorMessage}");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
                issue.CreatedByUserId = userId;
                issue.Status = "Açık";
                issue.CreatedAt = DateTime.UtcNow;
                issue.Id = _issueService.AddIssue(issue);
                _auditLogService.AddLog(new AuditLog {
                    UserId = userId,
                    Action = "Create",
                    EntityType = "Issue",
                    EntityId = issue.Id,
                    Timestamp = DateTime.UtcNow,
                    Description = $"Yeni ticket oluşturuldu: {issue.Title}"
                });
                // Tüm adminlere bildirim ve e-posta gönder
                var admins = _userService.GetAll().Where(u => u.Role == "Admin").ToList();
                var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                foreach (var admin in admins)
                {
                    _notificationService.AddNotification(new Notification {
                        UserId = admin.Id,
                        Message = $"Yeni bir ticket oluşturuldu: '{issue.Title}'",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone)
                    });
                    // E-posta gönder
                    if (!string.IsNullOrWhiteSpace(admin.Email))
                    {
                        _emailService.SendEmailAsync(
                            admin.Email,
                            "Yeni Ticket Oluşturuldu",
                            $"<b>Yeni bir ticket açıldı:</b> {issue.Title}<br>Açan kullanıcı: {User.Identity.Name}"
                        );
                    }
                }
                return RedirectToAction("Index");
            }
            return View("Create", issue);
        }
        public IActionResult Details(int id)
        {
            var issue = _issueService.GetById(id);
            if (issue == null) return NotFound();
            var createdBy = _userService.GetById(issue.CreatedByUserId)?.Username ?? "-";
            ViewBag.CreatedBy = createdBy;
            ViewBag.Comments = _commentService.GetCommentsByIssueId(id);
            ViewBag.Attachments = _attachmentService.GetAttachmentsByIssueId(id);
            var users = _userService.GetAll();
            ViewBag.UserDict = users.ToDictionary(u => u.Id, u => u.Username);
            ViewBag.SolutionNote = issue.SolutionNote;
            return View("Details", issue);
        }
        [HttpPost]
        public IActionResult Close(int id, string solutionNote)
        {
            var issue = _issueService.GetById(id);
            if (issue == null) return NotFound();
            var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            if (!User.IsInRole("Admin"))
                return Forbid();
            issue.Status = "Kapalı";
            issue.ClosedAt = DateTime.UtcNow;
            issue.SolutionNote = solutionNote;
            _issueService.UpdateIssue(issue);
            _auditLogService.AddLog(new AuditLog {
                UserId = userId,
                Action = "Close",
                EntityType = "Issue",
                EntityId = issue.Id,
                Timestamp = DateTime.UtcNow,
                Description = $"Ticket kapatıldı: {issue.Title} (Çözüm: {solutionNote})"
            });
            // Ticketı oluşturan kullanıcıya bildirim ve e-posta gönder
            var user = _userService.GetById(issue.CreatedByUserId);
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            _notificationService.AddNotification(new Notification {
                UserId = issue.CreatedByUserId,
                Message = $"Oluşturduğunuz '{issue.Title}' başlıklı ticket çözüme ulaştırıldı. Açıklama: {solutionNote}",
                IsRead = false,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone)
            });
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                _emailService.SendEmailAsync(
                    user.Email,
                    "Ticketınız Çözüldü ve Kapatıldı",
                    $"<b>Ticketınız çözüldü ve kapatıldı:</b> {issue.Title}<br>Çözüm: {solutionNote}"
                );
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddComment(int issueId, string content)
        {
            var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            if (!string.IsNullOrWhiteSpace(content))
            {
                _commentService.AddComment(new Models.IssueComment {
                    IssueId = issueId,
                    UserId = userId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                });
                // Eğer yorum admin tarafından eklendiyse, ticket sahibine bildirim ve e-posta gönder
                var issue = _issueService.GetById(issueId);
                var user = _userService.GetById(issue.CreatedByUserId);
                if (User.IsInRole("Admin") && user != null)
                {
                    var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                    _notificationService.AddNotification(new Notification {
                        UserId = user.Id,
                        Message = $"'{issue.Title}' başlıklı ticketınıza admin tarafından cevap verildi.",
                        IsRead = false,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone)
                    });
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        _emailService.SendEmailAsync(
                            user.Email,
                            "Ticketınıza Admin Cevabı",
                            $"<b>Ticketınıza admin tarafından cevap verildi:</b> {issue.Title}<br>Yorum: {content}"
                        );
                    }
                }
            }
            return RedirectToAction("Details", new { id = issueId });
        }

        [HttpPost]
        public IActionResult AddAttachment(int issueId)
        {
            var files = Request.Form.Files;
            var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var uploaded = new List<Models.IssueAttachment>();
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
                    var attachment = new Models.IssueAttachment {
                        IssueId = issueId,
                        FileName = fileName,
                        FilePath = "/uploads/" + fileName,
                        UploadedAt = DateTime.UtcNow,
                        UploadedByUserId = userId
                    };
                    _attachmentService.AddAttachment(attachment);
                    uploaded.Add(attachment);
                }
            }
            return RedirectToAction("Details", new { id = issueId });
        }

        [HttpPost]
        public IActionResult DeleteComment(int id, int issueId)
        {
            // Sadece yorumu ekleyen veya admin silebilir (geliştirilebilir)
            // Şimdilik herkes silebiliyor
            var comments = _commentService.GetCommentsByIssueId(issueId);
            var comment = comments.FirstOrDefault(c => c.Id == id);
            if (comment != null)
            {
                _commentService.DeleteComment(id);
            }
            return RedirectToAction("Details", new { id = issueId });
        }

        [HttpPost]
        public IActionResult DeleteAttachment(int id, int issueId)
        {
            // Sadece ekleyen veya admin silebilir (geliştirilebilir)
            // Şimdilik herkes silebiliyor
            var attachments = _attachmentService.GetAttachmentsByIssueId(issueId);
            var attachment = attachments.FirstOrDefault(a => a.Id == id);
            if (attachment != null)
            {
                _attachmentService.DeleteAttachment(id);
                // Dosyayı fiziksel olarak da silelim
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            return RedirectToAction("Details", new { id = issueId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var issue = _issueService.GetById(id);
            if (issue == null) return NotFound();
            var userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            // Sadece admin veya ticket sahibi silebilir
            if (!User.IsInRole("Admin") && issue.CreatedByUserId != userId)
                return Forbid();
            _issueService.DeleteIssue(id);
            _auditLogService.AddLog(new AuditLog {
                UserId = userId,
                Action = "Delete",
                EntityType = "Issue",
                EntityId = id,
                Timestamp = DateTime.UtcNow,
                Description = $"Ticket silindi: {issue.Title}"
            });
            return RedirectToAction("Index");
        }
    }
} 