using System.ComponentModel.DataAnnotations;

namespace ProjectAdoAssistant.Api.Configuration;

public sealed class FoundryAgentOptions
{
    public const string SectionName = "ProjectAdoAssistant:Foundry";

    [Required]
    public string Endpoint { get; init; } = string.Empty;

    [Required]
    public string SubscriptionId { get; init; } = string.Empty;

    [Required]
    public string ResourceGroupName { get; init; } = string.Empty;

    [Required]
    public string ProjectName { get; init; } = string.Empty;

    [Required]
    public string AgentId { get; init; } = string.Empty;

    [Range(100, 30_000)]
    public int RunPollingIntervalMs { get; init; } = 1000;

    [Range(10_000, 600_000)]
    public int RunTimeoutMs { get; init; } = 300_000; // 5 minutes
}
