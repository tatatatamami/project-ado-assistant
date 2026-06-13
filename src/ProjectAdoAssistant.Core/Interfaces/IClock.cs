namespace ProjectAdoAssistant.Core.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
