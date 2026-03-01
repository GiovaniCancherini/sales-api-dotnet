namespace SalesApi.Domain.Common;

public abstract record DomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}