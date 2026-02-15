namespace Snapflow.Common;

public interface IDomainEvent
{
    string? ConnectionId { get; init; }
}