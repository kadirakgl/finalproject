using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;
using TaskManagement.Models;

namespace TaskManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssueApiController : ControllerBase
    {
        private readonly IssueService _issueService;
        public IssueApiController(IssueService issueService)
        {
            _issueService = issueService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var issues = _issueService.GetAll();
            return Ok(issues);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var issue = _issueService.GetById(id);
            if (issue == null) return NotFound();
            return Ok(issue);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Issue issue)
        {
            var id = _issueService.AddIssue(issue);
            return CreatedAtAction(nameof(GetById), new { id }, issue);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Issue issue)
        {
            var existing = _issueService.GetById(id);
            if (existing == null) return NotFound();
            issue.Id = id;
            _issueService.UpdateIssue(issue);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _issueService.GetById(id);
            if (existing == null) return NotFound();
            _issueService.DeleteIssue(id);
            return NoContent();
        }
    }
} 