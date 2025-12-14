using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Core.Interfaces;

/// <summary>
/// Interface for AI provider implementations (ChatGPT, Claude, etc.)
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Gets the name of the AI provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends a request to the AI model and gets a response
    /// </summary>
    Task<AIResponse> SendRequestAsync(AIRequest request, CancellationToken cancellationToken = default);
}
