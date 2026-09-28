using SalesApi.Application.Sales.CreateSale;
using Bogus;

namespace SalesApi.Unit.Domain;

/// <summary>
/// Provides methods for generating test data for <see cref="CreateSaleHandler"/> tests
/// using the Bogus library, mirroring CreateUserHandlerTestData's pattern.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<CreateSaleItemCommand> createSaleItemFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid().ToString())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.ProductSku, f => f.Commerce.Ean8())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(1, 100));

    private static readonly Faker<CreateSaleCommand> createSaleHandlerFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleNumber, f => $"V-{f.Random.Number(1000, 9999)}")
        .RuleFor(s => s.Date, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.CustomerName, f => f.Name.FullName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.BranchCode, f => f.Random.AlphaNumeric(5).ToUpper())
        .RuleFor(s => s.Items, _ => createSaleItemFaker.Generate(1));

    /// <summary>
    /// Generates a valid CreateSaleCommand, with one valid item, and randomized data.
    /// </summary>
    public static CreateSaleCommand GenerateValidCommand()
    {
        return createSaleHandlerFaker.Generate();
    }
}
