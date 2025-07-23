using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Services;

namespace TaskManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly AuditLogService _auditLogService;
        private readonly EmailService _emailService;
        private readonly PasswordResetService _passwordResetService = new PasswordResetService();
        public AccountController(UserService userService, AuditLogService auditLogService, EmailService emailService)
        {
            _userService = userService;
            _auditLogService = auditLogService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _userService.GetByUsername(username);
            var hashMethod = _userService.GetType().GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (user == null || user.PasswordHash != hashMethod.Invoke(_userService, new object[] { password }).ToString())
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }
            if (!user.IsApproved)
            {
                ViewBag.Error = "Hesabınız henüz admin tarafından onaylanmadı.";
                return View();
            }
            // Kullanıcı başarılı giriş yaptıysa son aktif zamanı güncelle
            user.LastActiveAt = DateTime.Now;
            _userService.UpdateUser(user);
            // AuditLog: Login
            _auditLogService.AddLog(new TaskManagement.Models.AuditLog {
                UserId = user.Id,
                Action = "Login",
                EntityType = "User",
                EntityId = user.Id,
                Timestamp = DateTime.Now,
                Description = $"Kullanıcı giriş yaptı: {user.Username}"
            });
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var user = _userService.GetById(userId);
            if (user != null)
            {
                _auditLogService.AddLog(new TaskManagement.Models.AuditLog {
                    UserId = user.Id,
                    Action = "Logout",
                    EntityType = "User",
                    EntityId = user.Id,
                    Timestamp = DateTime.Now,
                    Description = $"Kullanıcı çıkış yaptı: {user.Username}"
                });
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId") ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var user = _userService.GetById(userId);
            if (user == null) return RedirectToAction("Login");
            ViewBag.Username = user.Username;
            ViewBag.Role = user.Role;
            ViewBag.Bio = user.Bio ?? "";
            ViewBag.Email = user.Email ?? "";
            var activities = _auditLogService.GetLogsByUserId(userId).Take(10).ToList();
            ViewBag.UserActivities = activities;
            return View("Profile");
        }
        [HttpPost]
        public IActionResult UpdateProfile(string email, string bio)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId") ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var user = _userService.GetById(userId);
            if (user == null) return RedirectToAction("Login");
            user.Email = email;
            user.Bio = bio;
            _userService.UpdateUser(user);
            TempData["ProfileUpdateSuccess"] = "Profiliniz güncellendi.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId") ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            var user = _userService.GetById(userId);
            var hashMethod = _userService.GetType().GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (user == null || user.PasswordHash != hashMethod.Invoke(_userService, new object[] { currentPassword }).ToString())
            {
                TempData["PasswordChangeError"] = "Mevcut şifreniz yanlış.";
                return RedirectToAction("Profile");
            }
            user.PasswordHash = hashMethod.Invoke(_userService, new object[] { newPassword }).ToString();
            _userService.DeleteUser(user.Id);
            _userService.Register(user.Username, newPassword, user.Role);
            TempData["PasswordChangeSuccess"] = "Şifreniz başarıyla değiştirildi.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string email)
        {
            var existing = _userService.GetByUsername(username);
            if (existing != null)
            {
                ViewBag.Error = "Bu kullanıcı adı zaten alınmış.";
                return View("Register");
            }
            var existingEmail = _userService.GetAll().FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
            if (existingEmail != null)
            {
                ViewBag.Error = "Bu e-posta adresiyle zaten bir kullanıcı kayıtlı.";
                return View("Register");
            }
            _userService.Register(username, password, "User", email);
            // Adminlere bildirim ve e-posta gönder
            var admins = _userService.GetAll().Where(u => u.Role == "Admin").ToList();
            foreach (var admin in admins)
            {
                // Bildirim
                var notificationService = new NotificationService();
                notificationService.AddNotification(new TaskManagement.Models.Notification {
                    UserId = admin.Id,
                    Message = $"Yeni kayıt başvurusu: {username} ({email})",
                    IsRead = false,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"))
                });
                // E-posta
                if (!string.IsNullOrWhiteSpace(admin.Email))
                {
                    _emailService.SendEmailAsync(
                        admin.Email,
                        "Yeni Kayıt Başvurusu",
                        $"<b>Yeni bir kullanıcı kayıt başvurusu yaptı:</b><br>Kullanıcı Adı: {username}<br>E-posta: {email}",
                        "kadirak.2001@hotmail.com",
                        "TaskManager Admin"
                    );
                }
            }
            TempData["RegisterSuccess"] = "Kayıt başvurunuz alınmıştır. Admin onayından sonra giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _userService.GetAll().FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                TempData["ForgotPasswordResult"] = "Bu e-posta adresiyle kayıtlı bir kullanıcı bulunamadı.";
                return RedirectToAction("Login");
            }
            // Token üret ve kaydet
            string token = _passwordResetService.GenerateToken();
            DateTime expiration = DateTime.UtcNow.AddHours(1);
            _passwordResetService.CreateToken(user.Id, token, expiration);
            // Şifre sıfırlama linki
            string resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
            string subject = "Şifre Sıfırlama Talebi";
            string html = $@"<b>Şifre sıfırlama talebiniz alındı.</b><br>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:<br><a href='{resetLink}'>Şifreyi Sıfırla</a><br><br>Bu e-posta otomatik olarak gönderilmiştir.";
            bool result = await _emailService.SendEmailAsync(email, subject, html, "kadirak.2001@hotmail.com", "TaskManager Admin");
            TempData["ForgotPasswordResult"] = result ? "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi." : "E-posta gönderilemedi. Lütfen tekrar deneyin.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var resetToken = _passwordResetService.GetByToken(token);
            if (resetToken == null || resetToken.Expiration < DateTime.UtcNow)
            {
                TempData["ForgotPasswordResult"] = "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.";
                return RedirectToAction("Login");
            }
            ViewBag.Token = token;
            return View("ResetPassword");
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            var resetToken = _passwordResetService.GetByToken(token);
            if (resetToken == null || resetToken.Expiration < DateTime.UtcNow)
            {
                TempData["ForgotPasswordResult"] = "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.";
                return RedirectToAction("Login");
            }
            var user = _userService.GetById(resetToken.UserId);
            if (user == null)
            {
                TempData["ForgotPasswordResult"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Login");
            }
            // Şifreyi güncelle
            var hashMethod = _userService.GetType().GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            user.PasswordHash = hashMethod.Invoke(_userService, new object[] { newPassword }).ToString();
            _userService.UpdateUser(user);
            _passwordResetService.DeleteByUserId(user.Id);
            TempData["ForgotPasswordResult"] = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            // Örnek e-posta adresi, gerçek sistemde kullanıcıdan alınmalı
            string to = "ornekmail@domain.com";
            string subject = "Test Mail - TaskManager";
            string html = "<b>Bu bir test mailidir.</b><br>Brevo API ile gönderildi.";
            bool result = await _emailService.SendEmailAsync(to, subject, html);
            ViewBag.EmailResult = result ? "Test maili başarıyla gönderildi." : "Test maili gönderilemedi.";
            return View("TestEmailResult");
        }
    }
} 