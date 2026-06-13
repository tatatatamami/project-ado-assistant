namespace ProjectAdoAssistant.Api.Models;

public sealed record ApiError(string Code, string Message, string TraceId);
