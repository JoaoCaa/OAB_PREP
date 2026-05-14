namespace OabPrep.Infrastructure.Services.Llm;

public enum LlmProvider { OpenAI, Anthropic, AzureOpenAI }

public sealed class LlmSettings
{
    public LlmProvider Provider { get; set; } = LlmProvider.OpenAI;
    public OpenAiSettings OpenAI { get; set; } = new();
    public AnthropicSettings Anthropic { get; set; } = new();
    public AzureOpenAiSettings AzureOpenAI { get; set; } = new();
}

public sealed class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}

public sealed class AnthropicSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-opus-4-7";
}

public sealed class AzureOpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-02-01";
}
