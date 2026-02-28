using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using Bogus;

namespace SalesApi.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating SaleItem test data.
/// Centralizes valid data generation for consistency across tests.
/// </summary>
public static class SaleItemTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a fully valid SaleItem with random values.
    /// </summary>
    public static SaleItem GenerateValidSaleItem()
    {
        return new SaleItem(
            ExternalProductTestData.GenerateValidExternalProduct(),
            Faker.Random.Number(1, 10),
            Faker.Random.Decimal(1, 100)
        );
    }

    /// <summary>
    /// Generates a valid SaleItem forcing a specific total amount.
    /// Quantity will be 1 and UnitPrice will match the provided price.
    /// </summary>
    public static SaleItem GenerateValidWithPrice(decimal totalPrice)
    {
        return new SaleItem(
            ExternalProductTestData.GenerateValidExternalProduct(),
            quantity: 1,
            unitPrice: totalPrice
        );
    }

    /// <summary>
    /// Generates a valid SaleItem with specific quantity and unit price.
    /// Useful for testing discount rules or calculations.
    /// </summary>
    public static SaleItem GenerateWithValues(
        int quantity,
        decimal unitPrice)
    {
        return new SaleItem(
            ExternalProductTestData.GenerateValidExternalProduct(),
            quantity,
            unitPrice
        );
    }
}