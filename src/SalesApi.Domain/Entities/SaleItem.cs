using SalesApi.Common.Security;
using SalesApi.Common.Validation;
using SalesApi.Domain.Common;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Validation;
using SalesApi.Domain.ValueObjects;

namespace SalesApi.Domain.Entities;


/// <summary>
/// Represents an item within a sale.
/// This entity belongs to the Sale aggregate and
/// encapsulates pricing and discount business rules.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets the unique identifier of the sale item.
    /// </summary>
    public Guid Id { get; private set; }

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
    /// Gets the discount amount applied to this item.
    /// Cannot be negative or greater than the item subtotal.
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

    private SaleItem() { } // EF

    public SaleItem(
        ExternalProduct product,
        int quantity,
        decimal unitPrice,
        decimal discount = 0)
    {
        Id = Guid.NewGuid();
        Product = product ?? throw new ArgumentNullException(nameof(product));
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
        IsCancelled = false;

        Validate();
        RecalculateTotal();
    }

    public void ApplyDiscount(decimal discount)
    {
        if (discount < 0)
        {
            throw new ArgumentException("Discount cannot be negative.");
        }

        Discount = discount;
        Validate();
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

        if (Discount > subtotal)
        {
            throw new InvalidOperationException("Discount cannot exceed subtotal.");
        }

        TotalAmount = subtotal - Discount;
    }

    private void Validate()
    {
        if (Product is null)
        {
            throw new ArgumentException("Product is required.");
        }

        if (Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        if (UnitPrice <= 0)
        {
            throw new ArgumentException("Unit price must be greater than zero.");
        }

        if (Discount < 0)
        {
            throw new ArgumentException("Discount cannot be negative.");
        }
    }

}