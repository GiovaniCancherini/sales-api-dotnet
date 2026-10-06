using SalesApi.Domain.Common;

namespace SalesApi.Domain.Events;

// Only the id: handlers that need more can load the user, and nothing sensitive
// (like the password hash) rides along with an event that may be logged or stored.
public sealed record UserRegisteredEvent(Guid UserId) : DomainEvent;
