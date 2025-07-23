using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;

namespace TaskManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly UserService _userService;
        public UserApiController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public IActionResult Create([FromBody] User user)
        {
            var id = _userService.AddUser(user);
            return CreatedAtAction(nameof(GetById), new { id }, user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] User user)
        {
            var existing = _userService.GetById(id);
            if (existing == null) return NotFound();
            user.Id = id;
            _userService.UpdateUser(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _userService.GetById(id);
            if (existing == null) return NotFound();
            _userService.DeleteUser(id);
            return NoContent();
        }
    }
} 