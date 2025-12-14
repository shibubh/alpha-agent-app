namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents a single task to be executed
/// </summary>
public class TaskItem
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public List<string>? Commands { get; init; }
    public string? PostCommand { get; init; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public string? Result { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Task execution status
/// </summary>
public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}
