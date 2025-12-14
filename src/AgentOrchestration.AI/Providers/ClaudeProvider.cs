using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentOrchestration.Core.Interfaces;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.AI.Providers;

/// <summary>
/// Claude (Anthropic) AI provider implementation
/// </summary>
public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private const string ApiEndpoint = "https://api.anthropic.com/v1/messages";

    public string ProviderName => "Claude";

    public ClaudeProvider(HttpClient httpClient, string apiKey, string model = "claude-3-5-sonnet-20241022")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = model;
    }

    public async Task<AIResponse> SendRequestAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new
            {
                model = _model,
                max_tokens = request.MaxTokens,
                temperature = request.Temperature,
                system = request.SystemMessage ?? string.Empty,
                messages = new[]
                {
                    new { role = "user", content = request.Prompt }
                }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint)
            {
                Content = JsonContent.Create(requestBody)
            };
            httpRequest.Headers.Add("x-api-key", _apiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);

            if (claudeResponse?.Content == null || claudeResponse.Content.Count == 0)
            {
                return new AIResponse
                {
                    Content = string.Empty,
                    Model = _model,
                    TokensUsed = 0,
                    Success = false,
                    ErrorMessage = "No response from Claude"
                };
            }

            return new AIResponse
            {
                Content = claudeResponse.Content[0].Text,
                Model = _model,
                TokensUsed = claudeResponse.Usage?.InputTokens + claudeResponse.Usage?.OutputTokens ?? 0,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new AIResponse
            {
                Content = string.Empty,
                Model = _model,
                TokensUsed = 0,
                Success = false,
                ErrorMessage = $"Error calling Claude: {ex.Message}"
            };
        }
    }

    private class ClaudeResponse
    {
        [JsonPropertyName("content")]
        public List<ContentBlock>? Content { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class ContentBlock
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public required string Text { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}
