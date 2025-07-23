using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;

namespace TaskManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectApiController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectApiController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var projects = _projectService.GetAll();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var project = _projectService.GetById(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Project project)
        {
            var id = _projectService.AddProject(project.Name, project.Description, project.OwnerId);
            return CreatedAtAction(nameof(GetById), new { id }, project);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Project project)
        {
            var existing = _projectService.GetById(id);
            if (existing == null) return NotFound();
            project.Id = id;
            _projectService.UpdateProject(project);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _projectService.GetById(id);
            if (existing == null) return NotFound();
            _projectService.DeleteProject(id);
            return NoContent();
        }

        [HttpGet("stats")]
        public IActionResult GetProjectStats()
        {
            // Örnek veri, ileride repository'den gerçek verilerle değiştirilebilir
            var stats = new {
                TotalProjects = 10,
                TotalTasks = 50,
                CompletedTasks = 30,
                TasksByUser = new[] {
                    new { Username = "Ali", TaskCount = 12 },
                    new { Username = "Ayşe", TaskCount = 8 },
                    new { Username = "Mehmet", TaskCount = 15 }
                },
                TasksByStatus = new[] {
                    new { Status = "Tamamlandı", Count = 30 },
                    new { Status = "Devam Ediyor", Count = 15 },
                    new { Status = "Beklemede", Count = 5 }
                }
            };
            return Ok(stats);
        }
    }
}
