using SalesApi.Domain.ValueObjects;
using Bogus;

namespace SalesApi.Unit.Domain.Entities.TestData;

public static class ExternalProductTestData
{
    private static readonly Faker Faker = new();

    public static ExternalProduct GenerateValidExternalProduct()
    {
        return new ExternalProduct(
            Guid.NewGuid(),
            Faker.Commerce.ProductName(),
            Faker.Random.AlphaNumeric(10)
        );
    }
}