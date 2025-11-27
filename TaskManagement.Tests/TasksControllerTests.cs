using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.API.Controllers;
using TaskManagement.Core.DTOs;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Models;
using TaskStatus = TaskManagement.Core.Models.TaskStatus;
namespace TaskManagement.Tests;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly TasksController _controller;
    public TasksControllerTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _controller = new TasksController(_mockRepository.Object);
    }
    [Fact]
    public async Task GetAllTasks_ReturnsOkResult_WithListOfTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
{
new TaskItem { Id = 1, Title = "Task 1", Status = TaskStatus.Pending },
new TaskItem { Id = 2, Title = "Task 2", Status = TaskStatus.InProgress }
};
        _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(tasks);
        // Act
        var result = await _controller.GetAllTasks();
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTasks =
        Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Equal(2, returnedTasks.Count());
    }
    [Fact]
    public async Task GetTask_WithValidId_ReturnsOkResult_WithTask()
    {
        // Arrange
        var taskId = 1;
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Status =
        TaskStatus.Pending
        };
        _mockRepository.Setup(repo => repo.GetByIdAsync(taskId)).ReturnsAsync(task);
        // Act
        var result = await _controller.GetTask(taskId);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal(taskId, returnedTask.Id);
        Assert.Equal("Test Task", returnedTask.Title);
    }
    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var taskId = 999;
        _mockRepository.Setup(repo =>
        repo.GetByIdAsync(taskId)).ReturnsAsync((TaskItem?)null);
        // Act
        var result = await _controller.GetTask(taskId);
        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var taskDto = new TaskItemCreateDto
        {
            Title = "New Task",
            Description = "Test Description",
            Status = TaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var createdTask = new TaskItem
        {
            Id = 1,
            Title = taskDto.Title,
            Description = taskDto.Description,
            Status = taskDto.Status,
            DueDate = taskDto.DueDate,
            CreatedAt = DateTime.UtcNow
        };
        _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<TaskItem>()))
        .ReturnsAsync(createdTask);
        // Act
        var result = await _controller.CreateTask(taskDto);
        // Assert
        var createdAtActionResult =
        Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedTask = Assert.IsType<TaskItem>(createdAtActionResult.Value);
        Assert.Equal("New Task", returnedTask.Title);
        Assert.Equal(1, returnedTask.Id);
    }
    [Fact]
    public async Task DeleteTask_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var taskId = 1;
        _mockRepository.Setup(repo => repo.DeleteAsync(taskId)).ReturnsAsync(true);
        // Act
        var result = await _controller.DeleteTask(taskId);
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var taskId = 999;
        _mockRepository.Setup(repo => repo.DeleteAsync(taskId)).ReturnsAsync(false);
        // Act
        var result = await _controller.DeleteTask(taskId);
        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var taskId = 1;
        var updateDto = new TaskItemUpdateDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Status = TaskStatus.Completed,
            DueDate = DateTime.UtcNow.AddDays(3)
        };
        var updatedTask = new TaskItem
        {
            Id = taskId,
            Title = updateDto.Title,
            Description = updateDto.Description,
            Status = updateDto.Status,
            DueDate = updateDto.DueDate,
            UpdatedAt = DateTime.UtcNow
        };
        _mockRepository.Setup(repo => repo.ExistsAsync(taskId)).ReturnsAsync(true);
        _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<TaskItem>()))
        .ReturnsAsync(updatedTask);
        // Act
        var result = await _controller.UpdateTask(taskId, updateDto);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("Updated Task", returnedTask.Title);
        Assert.Equal(TaskStatus.Completed, returnedTask.Status);
    }
}