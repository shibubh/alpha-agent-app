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

    public async Task<ExecutionPlan> CreatePlanAsync(string userPrompt, string? techStack = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
            throw new ArgumentException("User prompt cannot be empty", nameof(userPrompt));

        var systemMessage = @"You are an expert planning agent. Your job is to create detailed planning documents that describe WHAT should be in the final product and provide executable terminal commands when applicable.

Focus on:
- User-facing features and components
- Content structure and layout
- Visual elements and design details
- Functional requirements from user perspective
- Terminal commands for setup, installation, or execution steps
- Pre-commands for prerequisites and post-commands for verification

You must respond with a JSON object in the following format:
{
  ""description"": ""Overall plan description"",
  ""tasks"": [
    {
      ""title"": ""Task title"",
      ""description"": ""Detailed task description"",
      ""preCommand"": ""command to run before main task (optional, e.g., check prerequisites)"",
      ""command"": ""main terminal command to execute (optional, include when applicable)"",
      ""postCommand"": ""command to verify or run after main task (optional, e.g., verify installation)"",
      ""order"": 1
    }
  ]
}

Important guidelines:
1. Break down the requirements into clear, detailed specifications
2. Each task should describe a specific feature, component, or content area
3. Include terminal commands when tasks involve installation, setup, or execution (e.g., 'npm install express', 'dotnet new webapp', 'pip install flask')
4. Use preCommand for checking prerequisites (e.g., 'node --version', 'which python3', 'dotnet --version')
5. Use postCommand for verification (e.g., 'npm list express', 'flask --version', 'dotnet test', 'curl http://localhost:3000')
6. For descriptive/design tasks without commands, omit the command fields or leave them empty
7. Commands should be complete and executable as-is
8. Be specific about visual elements, content, and user interactions
9. Keep descriptions clear and detailed
10. Return ONLY valid JSON, no additional text or markdown";

        var prompt = $@"Please create a detailed planning document for the following requirement:

**User Requirement:**
{userPrompt}

Create a comprehensive detailed plan that describes what should be in the final product. Focus on features, components, content, and design elements - NOT on development or implementation steps.

For example, if creating a landing page:
- Describe header section (logo position, navigation items, etc.)
- Describe hero section (headline, image, call-to-action button, etc.)
- Describe feature blocks (how many, what content each should have, etc.)
- Describe footer (links, contact information, social media icons, etc.)

Be specific and detailed about WHAT should exist, not HOW to build it.";

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
            PreCommand = t.PreCommand,
            Command = t.Command,
            PostCommand = t.PostCommand,
            Order = t.Order > 0 ? t.Order : index + 1,
            Status = Core.Models.TaskStatus.Pending
        }).OrderBy(t => t.Order).ToList();

        return new ExecutionPlan
        {
            Id = Guid.NewGuid().ToString(),
            Goal = userPrompt,
            TechStack = techStack ?? "Not applicable - Planning phase",
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
        public string? PreCommand { get; set; }
        public string? Command { get; set; }
        public string? PostCommand { get; set; }
        public int Order { get; set; }
    }
}
