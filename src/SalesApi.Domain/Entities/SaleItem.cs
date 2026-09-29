using SalesApi.Common.Security;
using SalesApi.Common.Validation;
using SalesApi.Domain.Common;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Validation;
using SalesApi.Domain.ValueObjects;
using FluentValidation;

namespace SalesApi.Domain.Entities;

/// <summary>
/// Represents an item within a sale.
/// This entity belongs to the Sale aggregate and
/// encapsulates pricing and discount business rules.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Snapshot reference of the product at the time of sale.
    /// </summary>
    public ExternalProduct Product { get; private set; } = null!;

    /// <summary>
    /// Gets the quantity of the product in the sale.
    /// Must be greater than zero.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets the unit price of the product at the time of sale.
    /// Must be greater than zero.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Gets the discount amount applied to this item, computed automatically
    /// from Quantity - see the tiered discount rule in RecalculateTotal.
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// Gets the total monetary amount for this item.
    /// Calculated as (Quantity * UnitPrice) - Discount.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Indicates whether the item has been cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Maximum quantity of identical items allowed in a single sale line.
    /// </summary>
    private const int MaxQuantity = 20;

    /// <summary>
    /// Quantity thresholds and rates for the tiered discount business rule:
    /// below 4 items, no discount; 4-9 items, 10%; 10-20 items, 20%.
    /// </summary>
    private const int SecondTierMinQuantity = 10;
    private const int FirstTierMinQuantity = 4;
    private const decimal SecondTierDiscount = 0.20m;
    private const decimal FirstTierDiscount = 0.10m;

    private SaleItem() { } // Required by EF Core

    public SaleItem(
        ExternalProduct product,
        int quantity,
        decimal unitPrice)
    {
        if (quantity > MaxQuantity)
        {
            throw new InvalidOperationException($"Cannot sell more than {MaxQuantity} identical items.");
        }

        Id = Guid.NewGuid();
        Product = product ?? throw new ArgumentNullException(nameof(product));
        Quantity = quantity;
        UnitPrice = unitPrice;
        IsCancelled = false;

        RecalculateTotal();
    }

    public void Cancel()
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Item already cancelled.");
        }

        IsCancelled = true;
        TotalAmount = 0;
    }

    private void RecalculateTotal()
    {
        var subtotal = Quantity * UnitPrice;
        Discount = subtotal * GetDiscountRate(Quantity);
        TotalAmount = subtotal - Discount;
    }

    /// <summary>
    /// Quantity-based discount tiers (see README's Business Rules): below 4
    /// items no discount, 4-9 items 10%, 10-20 items 20%. Quantity above 20
    /// is rejected in the constructor, so it never reaches here.
    /// </summary>
    private static decimal GetDiscountRate(int quantity)
    {
        if (quantity >= SecondTierMinQuantity) return SecondTierDiscount;
        if (quantity >= FirstTierMinQuantity) return FirstTierDiscount;
        return 0m;
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleItemValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

}
