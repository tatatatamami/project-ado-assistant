namespace ProjectAdoAssistant.Core.Interfaces;

public interface IFoundryAgentClient
{
    Task<(string Content, string ThreadId)> SendMessageAsync(
        string userMessage,
        string? threadId,
        CancellationToken cancellationToken = default);
}
