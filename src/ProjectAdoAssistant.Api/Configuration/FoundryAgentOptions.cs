using System.ComponentModel.DataAnnotations;

namespace ProjectAdoAssistant.Api.Configuration;

public sealed class FoundryAgentOptions
{
    public const string SectionName = "ProjectAdoAssistant:Foundry";

    [Required]
    public string ProjectEndpoint { get; init; } = string.Empty;

    [Required]
    public string AgentName { get; init; } = string.Empty;

    [Required]
    public string AgentVersion { get; init; } = string.Empty;

    [Range(10_000, 600_000)]
    public int RequestTimeoutMs { get; init; } = 300_000; // 5 minutes
}
