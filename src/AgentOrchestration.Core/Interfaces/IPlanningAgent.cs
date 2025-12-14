using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Core.Interfaces;

/// <summary>
/// Interface for the planning agent that creates execution plans
/// </summary>
public interface IPlanningAgent
{
    /// <summary>
    /// Creates an execution plan based on user input and tech stack
    /// </summary>
    Task<ExecutionPlan> CreatePlanAsync(string userPrompt, string techStack, CancellationToken cancellationToken = default);
}
