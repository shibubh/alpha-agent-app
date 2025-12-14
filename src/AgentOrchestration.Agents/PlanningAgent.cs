using System.Text.Json;
using AgentOrchestration.Core.Interfaces;
using AgentOrchestration.Core.Models;

namespace AgentOrchestration.Agents;

/// <summary>
/// Planning agent that creates detailed execution plans
/// </summary>
public class PlanningAgent : IPlanningAgent
{
    private readonly IAIProvider _aiProvider;

    public PlanningAgent(IAIProvider aiProvider)
    {
        _aiProvider = aiProvider ?? throw new ArgumentNullException(nameof(aiProvider));
    }

    public async Task<ExecutionPlan> CreatePlanAsync(string userPrompt, string techStack, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            throw new ArgumentException("User prompt cannot be empty", nameof(userPrompt));

        if (string.IsNullOrWhiteSpace(techStack))
            throw new ArgumentException("Tech stack cannot be empty", nameof(techStack));

        var systemMessage = @"You are an expert project planning agent. Your job is to break down user requirements into detailed, actionable tasks.

You must respond with a JSON object in the following format:
{
  ""description"": ""Overall plan description"",
  ""tasks"": [
    {
      ""title"": ""Task title"",
      ""description"": ""Detailed task description"",
      ""order"": 1
    }
  ]
}

Important guidelines:
1. Break down the work into clear, sequential tasks
2. Each task should be specific and actionable
3. Consider the tech stack when planning tasks
4. Order tasks logically (setup -> implementation -> testing -> deployment)
5. Keep task descriptions clear and concise
6. Return ONLY valid JSON, no additional text or markdown";

        var prompt = $@"Please create a detailed execution plan for the following requirement:

**User Requirement:**
{userPrompt}

**Tech Stack:**
{techStack}

Create a comprehensive plan with numbered tasks that will accomplish this goal.";

        var aiRequest = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = systemMessage,
            Temperature = 0.7,
            MaxTokens = 2000
        };

        var response = await _aiProvider.SendRequestAsync(aiRequest, cancellationToken);

        if (!response.Success)
        {
            throw new InvalidOperationException($"Failed to create plan: {response.ErrorMessage}");
        }

        var planData = ParsePlanResponse(response.Content);

        var tasks = planData.Tasks.Select((t, index) => new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = t.Title,
            Description = t.Description,
            Order = t.Order > 0 ? t.Order : index + 1,
            Status = Core.Models.TaskStatus.Pending
        }).OrderBy(t => t.Order).ToList();

        return new ExecutionPlan
        {
            Id = Guid.NewGuid().ToString(),
            Goal = userPrompt,
            TechStack = techStack,
            Description = planData.Description,
            Tasks = tasks,
            Status = PlanStatus.Created
        };
    }

    private PlanData ParsePlanResponse(string response)
    {
        try
        {
            // Try to extract JSON from markdown code blocks if present
            var jsonContent = response.Trim();
            if (jsonContent.Contains("```json"))
            {
                var startIndex = jsonContent.IndexOf("```json") + 7;
                var endIndex = jsonContent.LastIndexOf("```");
                if (endIndex > startIndex)
                {
                    jsonContent = jsonContent.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            else if (jsonContent.Contains("```"))
            {
                var startIndex = jsonContent.IndexOf("```") + 3;
                var endIndex = jsonContent.LastIndexOf("```");
                if (endIndex > startIndex)
                {
                    jsonContent = jsonContent.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var planData = JsonSerializer.Deserialize<PlanData>(jsonContent, options);

            if (planData == null || planData.Tasks == null || planData.Tasks.Count == 0)
            {
                throw new InvalidOperationException("Invalid plan data received from AI");
            }

            return planData;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse AI response as JSON: {ex.Message}. Response: {response}");
        }
    }

    private class PlanData
    {
        public string Description { get; set; } = string.Empty;
        public List<TaskData> Tasks { get; set; } = new();
    }

    private class TaskData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
