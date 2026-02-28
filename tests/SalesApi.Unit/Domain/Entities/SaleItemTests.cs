using SalesApi.Domain.Entities;
using SalesApi.Domain.Events;
using SalesApi.Unit.Domain.Entities.TestData;
using Xunit;

namespace SalesApi.Unit.Domain.Entities;

/// <summary>
/// Unit tests for SaleItem entity.
/// Covers pricing rules, discount rules and cancellation behavior.
/// </summary>
public class SaleItemTests
{
    #region Constructor

    [Fact(DisplayName = "Given valid data When creating SaleItem Then total should be calculated correctly")]
    public void Given_ValidData_When_CreateSaleItem_Then_TotalShouldBeCalculated()
    {
        // Arrange
        var product = ExternalProductTestData.GenerateValidExternalProduct();
        var quantity = 2;
        var unitPrice = 10m;

        // Act
        var item = new SaleItem(product, quantity, unitPrice);

        // Assert
        Assert.Equal(20m, item.TotalAmount);
        Assert.False(item.IsCancelled);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
    }

    #endregion

    #region ApplyDiscount

    [Fact(DisplayName = "Given valid discount When applying Then total should be updated")]
    public void Given_ValidDiscount_When_ApplyDiscount_Then_TotalShouldBeUpdated()
    {
        // Arrange
        var item = SaleItemTestData.GenerateWithValues(2, 10m); // subtotal 20

        // Act
        item.ApplyDiscount(5m);

        // Assert
        Assert.Equal(5m, item.Discount);
        Assert.Equal(15m, item.TotalAmount);
    }

    [Fact(DisplayName = "Given negative discount When applying Then should throw exception")]
    public void Given_NegativeDiscount_When_ApplyDiscount_Then_ShouldThrowException()
    {
        // Arrange
        var item = SaleItemTestData.GenerateValidSaleItem();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => item.ApplyDiscount(-1m));
    }

    [Fact(DisplayName = "Given discount greater than subtotal When applying Then should throw exception")]
    public void Given_DiscountGreaterThanSubtotal_When_ApplyDiscount_Then_ShouldThrowException()
    {
        // Arrange
        var item = SaleItemTestData.GenerateWithValues(1, 10m); // subtotal 10

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => item.ApplyDiscount(20m));
    }

    #endregion

    #region Cancel

    [Fact(DisplayName = "Given active item When cancelling Then item should be cancelled and total zero")]
    public void Given_ActiveItem_When_Cancel_Then_ShouldBeCancelled()
    {
        // Arrange
        var item = SaleItemTestData.GenerateWithValues(2, 10m);

        // Act
        item.Cancel();

        // Assert
        Assert.True(item.IsCancelled);
        Assert.Equal(0m, item.TotalAmount);
    }

    [Fact(DisplayName = "Given cancelled item When cancelling again Then should throw exception")]
    public void Given_CancelledItem_When_CancelAgain_Then_ShouldThrowException()
    {
        // Arrange
        var item = SaleItemTestData.GenerateValidSaleItem();
        item.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => item.Cancel());
    }

    #endregion

    #region Validation

    [Fact(DisplayName = "Given valid item When validating Then result should be valid")]
    public void Given_ValidItem_When_Validate_Then_ShouldBeValid()
    {
        // Arrange
        var item = SaleItemTestData.GenerateValidSaleItem();

        // Act
        var result = item.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Given quantity zero When validating Then should be invalid")]
    public void Given_QuantityZero_When_Validate_Then_ShouldBeInvalid()
    {
        // Arrange
        var product = ExternalProductTestData.GenerateValidExternalProduct();
        var item = new SaleItem(product, 0, 10m);

        // Act
        var result = item.Validate();

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "Given unit price zero When validating Then should be invalid")]
    public void Given_UnitPriceZero_When_Validate_Then_ShouldBeInvalid()
    {
        // Arrange
        var product = ExternalProductTestData.GenerateValidExternalProduct();
        var item = new SaleItem(product, 1, 0m);

        // Act
        var result = item.Validate();

        // Assert
        Assert.False(result.IsValid);
    }

    #endregion

    #region Business Calculation

    [Fact(DisplayName = "Given quantity and price When creating Then total equals quantity multiplied by price")]
    public void Given_QuantityAndPrice_When_Create_Then_TotalEqualsQuantityTimesPrice()
    {
        // Arrange
        var quantity = 5;
        var unitPrice = 3m;
        var product = ExternalProductTestData.GenerateValidExternalProduct();

        // Act
        var item = new SaleItem(product, quantity, unitPrice);

        // Assert
        Assert.Equal(15m, item.TotalAmount);
    }

    #endregion
}