namespace SalesApi.Domain.Common;

public class BaseEntity : IComparable<BaseEntity>
{
    // init: settable by the entity's own constructor (or an object initializer), never changed afterwards.
    public Guid Id { get; init; }

    public int CompareTo(BaseEntity? other)
    {
        if (other == null)
        {
            return 1;
        }

        return other!.Id.CompareTo(Id);
    }
}
