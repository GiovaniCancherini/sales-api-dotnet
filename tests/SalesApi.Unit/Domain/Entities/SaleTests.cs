using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Events;
using SalesApi.Unit.Domain.Entities.TestData;
using Xunit;

namespace SalesApi.Unit.Domain.Entities;

/// <summary>
/// Unit tests for Sale aggregate root.
/// Covers invariants, behaviors and business rules.
/// </summary>
public class SaleTests
{
    #region Create

    [Fact(DisplayName = "Given valid data When creating sale Then status should be Active")]
    public void Given_ValidData_When_CreateSale_Then_StatusShouldBeActive()
    {
        // Arrange
        var saleNumber = "S-123";
        var customer = ExternalCustomerTestData.GenerateValidExternalCustomer();
        var branch = ExternalBranchTestData.GenerateValidExternalBranch();

        // Act
        var sale = Sale.Create(saleNumber, customer, branch);

        // Assert
        Assert.Equal(SaleStatus.Active, sale.Status);
        Assert.Equal(saleNumber, sale.SaleNumber);
        Assert.Equal(customer, sale.Customer);
        Assert.Equal(branch, sale.Branch);
        Assert.Empty(sale.Items);
        Assert.Equal(0m, sale.TotalAmount);
    }

    #endregion

    #region AddItem

    [Fact(DisplayName = "Given active sale When adding item Then total amount should be updated")]
    public void Given_ActiveSale_When_AddItem_Then_TotalAmountShouldBeUpdated()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleItemTestData.GenerateValidSaleItem();

        // Act
        sale.AddItem(item);

        // Assert
        Assert.Single(sale.Items);
        Assert.Equal(item.TotalAmount, sale.TotalAmount);
    }

    [Fact(DisplayName = "Given cancelled sale When adding item Then should throw exception")]
    public void Given_CancelledSale_When_AddItem_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleItemTestData.GenerateValidSaleItem();
        sale.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sale.AddItem(item));
    }

    #endregion

    #region Cancel

    [Fact(DisplayName = "Given active sale When cancelling Then status should change to Cancelled")]
    public void Given_ActiveSale_When_Cancel_Then_StatusShouldBeCancelled()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.Cancel();

        // Assert
        Assert.Equal(SaleStatus.Cancelled, sale.Status);
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling again Then should throw exception")]
    public void Given_CancelledSale_When_CancelAgain_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sale.Cancel());
    }

    #endregion

    #region CancelItem

    [Fact(DisplayName = "Given sale with item When cancelling item Then item should be cancelled")]
    public void Given_SaleWithItem_When_CancelItem_Then_ItemShouldBeCancelled()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleItemTestData.GenerateValidSaleItem();
        sale.AddItem(item);

        // Act
        sale.CancelItem(item.Id);

        // Assert
        Assert.True(item.IsCancelled); // assuming this property exists
    }

    [Fact(DisplayName = "Given sale without item When cancelling item Then should throw exception")]
    public void Given_SaleWithoutItem_When_CancelItem_Then_ShouldThrowException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sale.CancelItem(Guid.NewGuid()));
    }

    #endregion

    #region Total Calculation

    [Fact(DisplayName = "Given multiple items When adding Then total should be sum of all items")]
    public void Given_MultipleItems_When_Adding_Then_TotalShouldBeSum()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item1 = SaleItemTestData.GenerateValidWithPrice(10m);
        var item2 = SaleItemTestData.GenerateValidWithPrice(20m);

        // Act
        sale.AddItem(item1);
        sale.AddItem(item2);

        // Assert
        Assert.Equal(30m, sale.TotalAmount);
    }

    #endregion

    #region Validation

    [Fact(DisplayName = "Given valid sale When validating Then result should be valid")]
    public void Given_ValidSale_When_Validate_Then_ShouldBeValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSaleWithItem();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Given sale without items When validating Then should be invalid")]
    public void Given_SaleWithoutItems_When_Validate_Then_ShouldBeInvalid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Detail.Contains("must contain at least one item"));
    }

    [Fact(DisplayName = "Given sale without sale number When validating Then should be invalid")]
    public void Given_SaleWithoutSaleNumber_When_Validate_Then_ShouldBeInvalid()
    {
        // Arrange
        var sale = new Sale(
            "",
            ExternalCustomerTestData.GenerateValidExternalCustomer(),
            ExternalBranchTestData.GenerateValidExternalBranch()
        );

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
    }

    #endregion

    #region Domain Events

    [Fact(DisplayName = "Given valid data When creating sale Then should raise SaleCreatedEvent")]
    public void Given_ValidData_When_CreateSale_Then_ShouldRaiseSaleCreatedEvent()
    {
        // Arrange
        var saleNumber = "S-123";
        var customer = ExternalCustomerTestData.GenerateValidExternalCustomer();
        var branch = ExternalBranchTestData.GenerateValidExternalBranch();

        // Act
        var sale = Sale.Create(saleNumber, customer, branch);

        // Assert
        var domainEvent = Assert.Single(sale.Events);
        Assert.IsType<SaleCreatedEvent>(domainEvent);
    }

    [Fact(DisplayName = "Given active sale When adding item Then should raise SaleModifiedEvent")]
    public void Given_ActiveSale_When_AddItem_Then_ShouldRaiseSaleModifiedEvent()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleItemTestData.GenerateValidSaleItem();

        sale.ClearEvents(); // removes event from Create

        // Act
        sale.AddItem(item);

        // Assert
        var domainEvent = Assert.Single(sale.Events);
        Assert.IsType<SaleModifiedEvent>(domainEvent);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then should raise SaleCancelledEvent")]
    public void Given_ActiveSale_When_Cancel_Then_ShouldRaiseSaleCancelledEvent()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        sale.ClearEvents();

        // Act
        sale.Cancel();

        // Assert
        var domainEvent = Assert.Single(sale.Events);
        Assert.IsType<SaleCancelledEvent>(domainEvent);
    }

    [Fact(DisplayName = "Given sale with item When cancelling item Then should raise ItemCancelledEvent")]
    public void Given_SaleWithItem_When_CancelItem_Then_ShouldRaiseItemCancelledEvent()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var item = SaleItemTestData.GenerateValidSaleItem();

        sale.AddItem(item);
        sale.ClearEvents();

        // Act
        sale.CancelItem(item.Id);

        // Assert
        var domainEvent = Assert.Single(sale.Events);
        Assert.IsType<ItemCancelledEvent>(domainEvent);
    }

    #endregion
}