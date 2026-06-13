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

    public int RunPollingIntervalMs { get; init; } = 1000;
}
