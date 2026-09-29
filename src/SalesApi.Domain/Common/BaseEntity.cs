using SalesApi.Common.Validation;

namespace SalesApi.Domain.Common;

public class BaseEntity : IComparable<BaseEntity>
{
    // init: settable by the entity's own constructor (or an object initializer), never changed afterwards.
    public Guid Id { get; init; }

    public Task<IEnumerable<ValidationErrorDetail>> ValidateAsync()
    {
        return Validator.ValidateAsync(this);
    }

    public int CompareTo(BaseEntity? other)
    {
        if (other == null)
        {
            return 1;
        }

        return other!.Id.CompareTo(Id);
    }
}
