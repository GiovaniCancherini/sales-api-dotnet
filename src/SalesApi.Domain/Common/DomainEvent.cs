using MediatR;

namespace SalesApi.Domain.Common;

public abstract record DomainEvent : INotification
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
