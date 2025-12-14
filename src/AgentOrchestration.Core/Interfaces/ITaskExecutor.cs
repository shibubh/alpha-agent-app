using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Core.Interfaces;

/// <summary>
/// Interface for executing tasks from an execution plan
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// Executes a single task
    /// </summary>
    Task<TaskItem> ExecuteTaskAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes all tasks in a plan sequentially
    /// </summary>
    Task<ExecutionPlan> ExecutePlanAsync(ExecutionPlan plan, CancellationToken cancellationToken = default);
}
