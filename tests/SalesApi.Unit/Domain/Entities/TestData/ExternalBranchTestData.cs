using SalesApi.Domain.ValueObjects;
using Bogus;

namespace SalesApi.Unit.Domain.Entities.TestData;

public static class ExternalBranchTestData
{
    private static readonly Faker Faker = new();

    public static ExternalBranch GenerateValidExternalBranch()
    {
        return new ExternalBranch(
            Guid.NewGuid(),
            Faker.Company.CompanyName(),
            Faker.Random.AlphaNumeric(10)
        );
    }
}