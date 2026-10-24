namespace TaskManager.Api.Data;
public sealed class TaskEntity
{
 public Guid Id { get; set; }
 public required string Title { get; set; }
 public string? Description { get; set; }
 public TaskState Status { get; set; } = TaskState.Todo;
 public DateTimeOffset CreatedAt { get; set; }
 public DateTimeOffset? DueDate { get; set; }
}
