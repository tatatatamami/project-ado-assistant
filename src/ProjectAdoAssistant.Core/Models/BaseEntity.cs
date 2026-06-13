namespace ProjectAdoAssistant.Core.Models;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
