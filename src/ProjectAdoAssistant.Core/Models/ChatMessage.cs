namespace ProjectAdoAssistant.Core.Models;

public enum ChatRole
{
    User,
    Assistant,
}

public sealed class ChatMessage : BaseEntity
{
    public required ChatRole Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
