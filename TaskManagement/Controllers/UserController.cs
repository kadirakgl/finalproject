using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            var users = _userService.GetAll();
            return View("Index", users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                _userService.Register(user.Username, user.PasswordHash, user.Role);
                return RedirectToAction("Index");
            }
            return View("Create", user);
        }

        public IActionResult Delete(int id)
        {
            _userService.DeleteUser(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult PendingApprovals()
        {
            var users = _userService.GetAll().Where(u => !u.IsApproved).ToList();
            return View("PendingApprovals", users);
        }
        [HttpPost]
        public IActionResult Approve(int id)
        {
            _userService.ApproveUser(id);
            return RedirectToAction("PendingApprovals");
        }
        [HttpPost]
        public IActionResult Reject(int id)
        {
            _userService.RejectUser(id);
            return RedirectToAction("PendingApprovals");
        }
    }
} 