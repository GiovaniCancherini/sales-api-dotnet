using SalesApi.Domain.Entities;
using SalesApi.Domain.Common;

namespace SalesApi.Domain.Events
{
    public sealed record UserRegisteredEvent (User user) : DomainEvent;
}
