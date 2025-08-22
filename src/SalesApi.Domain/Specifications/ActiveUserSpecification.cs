using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;

namespace SalesApi.Domain.Specifications;

public class ActiveUserSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User user)
    {
        return user.Status == UserStatus.Active;
    }
}
