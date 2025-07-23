using TaskManagement.DataAccess;
using TaskManagement.Models;
using System.Collections.Generic;

namespace TaskManagement.Services
{
    public class ProjectService
    {
        private readonly ProjectRepository _projectRepository;

        public ProjectService(ProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public int AddProject(string name, string description, int ownerId) => _projectRepository.Add(new Project { Name = name, Description = description, OwnerId = ownerId });

        public Project GetById(int id) => _projectRepository.GetById(id);

        public List<Project> GetAll() => _projectRepository.GetAll();

        public List<Project> GetByOwnerId(int ownerId)
        {
            return _projectRepository.GetByOwnerId(ownerId);
        }

        public void UpdateProject(Project project) => _projectRepository.Update(project);
        public void DeleteProject(int id) => _projectRepository.Delete(id);
    }
}
