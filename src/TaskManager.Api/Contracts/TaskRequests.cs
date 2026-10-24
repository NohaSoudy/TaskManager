using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Data;
namespace TaskManager.Api.Contracts;
public sealed class CreateTaskRequest { [Required, NotWhiteSpace] public string Title { get; init; } = string.Empty; public string? Description { get; init; } public DateTimeOffset? DueDate { get; init; } }
public sealed class UpdateTaskRequest { [Required, NotWhiteSpace] public string Title { get; init; } = string.Empty; public string? Description { get; init; } [Required, EnumDataType(typeof(TaskState))] public TaskState? Status { get; init; } public DateTimeOffset? DueDate { get; init; } }
public sealed class ChangeTaskStateRequest { [Required, EnumDataType(typeof(TaskState))] public TaskState? Status { get; init; } }
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotWhiteSpaceAttribute : ValidationAttribute { public override bool IsValid(object? value) => value is string s && !string.IsNullOrWhiteSpace(s); }
