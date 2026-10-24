using TaskManager.Api.Data;
namespace TaskManager.Api.Contracts;
public sealed record TaskResponse(Guid Id, string Title, string? Description, TaskState Status, DateTimeOffset CreatedAt, DateTimeOffset? DueDate)
{ public static TaskResponse From(TaskEntity t) => new(t.Id, t.Title, t.Description, t.Status, t.CreatedAt, t.DueDate); }
