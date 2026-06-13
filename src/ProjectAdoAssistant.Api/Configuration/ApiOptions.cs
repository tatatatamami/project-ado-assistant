using System.ComponentModel.DataAnnotations;

namespace ProjectAdoAssistant.Api.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "ProjectAdoAssistant:Api";

    [Required]
    public string ServiceName { get; init; } = "ProjectAdoAssistant.Api";
}
