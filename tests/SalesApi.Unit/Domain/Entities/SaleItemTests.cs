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

    #region Discount tiers (README Business Rules)

    [Fact(DisplayName = "Given quantity below 4 When creating Then no discount is applied")]
    public void Given_QuantityBelowFour_When_Create_Then_NoDiscount()
    {
        // Arrange & Act
        var item = SaleItemTestData.GenerateWithValues(3, 10m); // subtotal 30

        // Assert
        Assert.Equal(0m, item.Discount);
        Assert.Equal(30m, item.TotalAmount);
    }

    [Fact(DisplayName = "Given quantity between 4 and 9 When creating Then a 10% discount is applied")]
    public void Given_QuantityBetweenFourAndNine_When_Create_Then_TenPercentDiscount()
    {
        // Arrange & Act
        var item = SaleItemTestData.GenerateWithValues(4, 10m); // subtotal 40

        // Assert
        Assert.Equal(4m, item.Discount); // 10% of 40
        Assert.Equal(36m, item.TotalAmount);
    }

    [Fact(DisplayName = "Given quantity between 10 and 20 When creating Then a 20% discount is applied")]
    public void Given_QuantityBetweenTenAndTwenty_When_Create_Then_TwentyPercentDiscount()
    {
        // Arrange & Act
        var item = SaleItemTestData.GenerateWithValues(10, 10m); // subtotal 100

        // Assert
        Assert.Equal(20m, item.Discount); // 20% of 100
        Assert.Equal(80m, item.TotalAmount);
    }

    [Fact(DisplayName = "Given quantity above 20 When creating Then should throw exception")]
    public void Given_QuantityAboveTwenty_When_Create_Then_ShouldThrowException()
    {
        // Arrange
        var product = ExternalProductTestData.GenerateValidExternalProduct();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new SaleItem(product, 21, 10m));
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

    [Fact(DisplayName = "Given quantity and price below the discount threshold When creating Then total equals quantity multiplied by price")]
    public void Given_QuantityAndPrice_When_Create_Then_TotalEqualsQuantityTimesPrice()
    {
        // Arrange - quantity kept below 4 so no discount tier interferes with this check
        var quantity = 2;
        var unitPrice = 3m;
        var product = ExternalProductTestData.GenerateValidExternalProduct();

        // Act
        var item = new SaleItem(product, quantity, unitPrice);

        // Assert
        Assert.Equal(6m, item.TotalAmount);
    }

    #endregion
}