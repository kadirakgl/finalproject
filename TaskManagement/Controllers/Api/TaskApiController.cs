using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;
using System;
using Task = TaskManagement.Models.Task;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskApiController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskApiController(TaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Tüm görevleri getirir.
        /// </summary>
        /// <returns>Görev listesi</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "JwtBearer")]
        public IActionResult GetAll()
        {
            var tasks = _taskService.GetAll();
            return Ok(tasks);
        }

        /// <summary>
        /// Yeni bir görev ekler.
        /// </summary>
        /// <param name="task">Eklenecek görev nesnesi</param>
        /// <returns>Eklenen görevin Id'si</returns>
        [HttpPost]
        public IActionResult Create([FromBody] TaskManagement.Models.Task task)
        {
            var id = _taskService.AddTask(task.ProjectId, task.Title, task.Description, task.Status, task.AssignedUserId ?? 0, task.CreatedAt, task.DueDate);
            return CreatedAtAction(nameof(GetById), new { id }, task);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var task = _taskService.GetById(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TaskManagement.Models.Task task)
        {
            var existing = _taskService.GetById(id);
            if (existing == null) return NotFound();
            task.Id = id;
            _taskService.UpdateTask(task);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _taskService.GetById(id);
            if (existing == null) return NotFound();
            _taskService.DeleteTask(id);
            return NoContent();
        }
    }
}
