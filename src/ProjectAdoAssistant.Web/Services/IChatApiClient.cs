using ProjectAdoAssistant.Core.Dtos;

namespace ProjectAdoAssistant.Web.Services;

public interface IChatApiClient
{
    Task<ChatResponseDto> SendMessageAsync(
        string userMessage,
        string? threadId,
        CancellationToken cancellationToken = default);
}
