namespace AgentOrchestration.Core.Models;

/// <summary>
/// Represents a complete execution plan with tasks
/// </summary>
public class ExecutionPlan
{
    public required string Id { get; init; }
    public required string Goal { get; init; }
    public required string TechStack { get; init; }
    public required string Description { get; init; }
    public required List<TaskItem> Tasks { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public PlanStatus Status { get; set; } = PlanStatus.Created;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Plan execution status
/// </summary>
public enum PlanStatus
{
    Created,
    InProgress,
    Completed,
    Failed
}
