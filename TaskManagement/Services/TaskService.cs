using TaskManagement.DataAccess;
using TaskManagement.Models;
using System;
using System.Collections.Generic;
using Task = TaskManagement.Models.Task;

namespace TaskManagement.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepository;

        public TaskService(TaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public int AddTask(int projectId, string title, string description, string status, int assignedUserId, DateTime createdAt, DateTime? dueDate) => _taskRepository.Add(new Models.Task { ProjectId = projectId, Title = title, Description = description, Status = status, AssignedUserId = assignedUserId, CreatedAt = createdAt, DueDate = dueDate });

        public Models.Task GetById(int id) => _taskRepository.GetById(id);

        public List<Models.Task> GetAll() => _taskRepository.GetAll();

        public List<Task> GetByProjectId(int projectId)
        {
            return _taskRepository.GetByProjectId(projectId);
        }

        public List<Task> GetByAssignedUserId(int userId)
        {
            return _taskRepository.GetByAssignedUserId(userId);
        }

        public void UpdateTask(Models.Task task) => _taskRepository.Update(task);
        public void DeleteTask(int id) => _taskRepository.Delete(id);
    }
}
