using SalesApi.Domain.ValueObjects;
using Bogus;

namespace SalesApi.Unit.Domain.Entities.TestData;

public static class ExternalCustomerTestData
{
    private static readonly Faker Faker = new();

    public static ExternalCustomer GenerateValidExternalCustomer()
    {
        return new ExternalCustomer(
            Guid.NewGuid(),
            Faker.Person.FullName
        );
    }
}