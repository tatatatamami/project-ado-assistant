namespace ProjectAdoAssistant.Core.Dtos;

public sealed record HealthCheckDto(string Status, string ServiceName, string Environment);
