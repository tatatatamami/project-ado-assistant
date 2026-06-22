using System.Net.Http.Json;
using ProjectAdoAssistant.Core.Dtos;

namespace ProjectAdoAssistant.Web.Services;

public sealed class ChatApiClient : IChatApiClient
{
    private readonly HttpClient _httpClient;

    public ChatApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChatResponseDto> SendMessageAsync(
        string userMessage,
        string? conversationId,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatRequestDto(userMessage, conversationId);
        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content
            .ReadFromJsonAsync<ApiResponse<ChatResponseDto>>(cancellationToken);

        if (apiResponse is not { Success: true, Data: not null })
        {
            var errorMessage = apiResponse?.Error?.Message
                ?? $"Unexpected API response (HTTP {(int)response.StatusCode}).";
            throw new InvalidOperationException(errorMessage);
        }

        return apiResponse.Data;
    }

    private sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error);
    private sealed record ApiError(string Code, string Message, string TraceId);
}
