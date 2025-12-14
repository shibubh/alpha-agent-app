namespace AgentOrchestration.Core.Models;

/// <summary>
/// Request to AI model
/// </summary>
public class AIRequest
{
    public required string Prompt { get; init; }
    public string? SystemMessage { get; init; }
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2000;
}

/// <summary>
/// Response from AI model
/// </summary>
public class AIResponse
{
    public required string Content { get; init; }
    public required string Model { get; init; }
    public int TokensUsed { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
