using ProjectAdoAssistant.Core.Dtos;

namespace ProjectAdoAssistant.Core.Interfaces;

public interface IFoundryAgentClient
{
    Task<ChatResponseDto> SendMessageAsync(
        string userMessage,
        string? conversationId,
        CancellationToken cancellationToken = default);
}
