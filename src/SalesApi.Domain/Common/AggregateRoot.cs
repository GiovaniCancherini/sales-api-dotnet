using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApi.Domain.Common;

public abstract class AggregateRoot : BaseEntity
{
    private readonly List<DomainEvent> _events = new();

    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    protected void Raise(DomainEvent @event)
    {
        _events.Add(@event);
    }

    public void ClearEvents()
    {
        _events.Clear();
    }
}