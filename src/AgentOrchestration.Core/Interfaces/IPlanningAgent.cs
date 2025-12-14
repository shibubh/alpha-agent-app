using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Core.Interfaces;

/// <summary>
/// Interface for the planning agent that creates detailed planning documents
/// </summary>
public interface IPlanningAgent
{
    /// <summary>
    /// Creates a detailed planning document based on user requirements
    /// Focuses on WHAT to build (features, UI, content) not HOW to build it
    /// </summary>
    Task<ExecutionPlan> CreatePlanAsync(string userPrompt, string? techStack = null, CancellationToken cancellationToken = default);
}
