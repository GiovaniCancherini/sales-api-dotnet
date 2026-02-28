using SalesApi.Domain.Entities;

namespace SalesApi.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating valid Sale aggregates for testing.
/// </summary>
public static class SaleTestData
{
    public static Sale GenerateValidSale()
    {
        return Sale.Create(
            saleNumber: Guid.NewGuid().ToString(),
            customer: ExternalCustomerTestData.GenerateValidExternalCustomer(),
            branch: ExternalBranchTestData.GenerateValidExternalBranch()
        );
    }

    public static Sale GenerateValidSaleWithItem()
    {
        var sale = GenerateValidSale();
        sale.AddItem(SaleItemTestData.GenerateValidSaleItem());
        sale.ClearEvents(); // clears events from Create/Add
        return sale;
    }
}