using Xunit;
using TaskManagement.Services;
using TaskManagement.Models;
using System.Collections.Generic;
using Moq;
using TaskManagement.DataAccess;
using System;

namespace TaskManagement.Tests
{
    public class TaskServiceTests
    {
        [Fact]
        public void AddTask_ShouldAddTaskSuccessfully()
        {
            // Arrange
            var mockTaskRepo = new Mock<TaskRepository>(null);
            var mockProjectRepo = new Mock<ProjectRepository>(null);
            var mockUserRepo = new Mock<UserRepository>(null);
            var mockCommentRepo = new Mock<CommentRepository>(null);
            var mockAttachmentRepo = new Mock<AttachmentRepository>(null);
            var service = new TaskService(mockTaskRepo.Object, mockProjectRepo.Object, mockUserRepo.Object, mockCommentRepo.Object, mockAttachmentRepo.Object);
            var projectId = 1;
            var title = "Test Görevi";
            var description = "Açıklama";
            var status = "Yapılacak";
            int? assignedUserId = 2;
            var createdAt = DateTime.UtcNow;
            DateTime? dueDate = DateTime.UtcNow.AddDays(3);

            mockTaskRepo.Setup(r => r.Add(It.IsAny<Task>())).Returns(42);

            // Act
            var resultId = service.AddTask(projectId, title, description, status, assignedUserId, createdAt, dueDate);

            // Assert
            Assert.Equal(42, resultId);
            mockTaskRepo.Verify(r => r.Add(It.Is<Task>(t => t.Title == title && t.ProjectId == projectId)), Times.Once);
        }
    }
} 