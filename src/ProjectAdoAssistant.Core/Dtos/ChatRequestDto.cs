using System.ComponentModel.DataAnnotations;

namespace ProjectAdoAssistant.Core.Dtos;

public sealed record ChatRequestDto(
    [Required, MinLength(1)] string UserMessage,
    string? ConversationId);
