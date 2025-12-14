using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentOrchestration.Core.Interfaces;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.AI.Providers;

/// <summary>
/// ChatGPT (OpenAI) AI provider implementation
/// </summary>
public class ChatGPTProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private const string ApiEndpoint = "https://api.openai.com/v1/chat/completions";

    public string ProviderName => "ChatGPT";

    public ChatGPTProvider(HttpClient httpClient, string apiKey, string model = "gpt-4")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = model;
    }

    public async Task<AIResponse> SendRequestAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrEmpty(request.SystemMessage))
            {
                messages.Add(new ChatMessage { Role = "system", Content = request.SystemMessage });
            }

            messages.Add(new ChatMessage { Role = "user", Content = request.Prompt });

            var requestBody = new
            {
                model = _model,
                messages = messages,
                temperature = request.Temperature,
                max_tokens = request.MaxTokens
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint)
            {
                Content = JsonContent.Create(requestBody)
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var chatResponse = JsonSerializer.Deserialize<ChatGPTResponse>(responseContent);

            if (chatResponse?.Choices == null || chatResponse.Choices.Count == 0)
            {
                return new AIResponse
                {
                    Content = string.Empty,
                    Model = _model,
                    TokensUsed = 0,
                    Success = false,
                    ErrorMessage = "No response from ChatGPT"
                };
            }

            return new AIResponse
            {
                Content = chatResponse.Choices[0].Message.Content,
                Model = _model,
                TokensUsed = chatResponse.Usage?.TotalTokens ?? 0,
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
                ErrorMessage = $"Error calling ChatGPT: {ex.Message}"
            };
        }
    }

    private class ChatMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }

    private class ChatGPTResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public required ChatMessage Message { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
