using SalesApi.Common.Security;
using SalesApi.Common.Validation;
using SalesApi.Domain.Common;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Validation;
using SalesApi.Domain.ValueObjects;

namespace SalesApi.Domain.Entities;


/// <summary>
/// Represents a sale transaction within the system.
/// This aggregate root encapsulates sale behavior, invariants,
/// and business rules following Domain-Driven Design principles.
/// </summary>
public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = new();

    /// <summary>
    /// Gets the unique identifier of the sale.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the business sale number.
    /// This value is generated externally and must be unique.
    /// </summary>
    public string SaleNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date and time when the sale was created.
    /// Stored as UTC.
    /// </summary>
    public DateTimeOffset Date { get; private set; }

    /// <summary>
    /// Snapshot reference of the customer at the time of sale.
    /// </summary>
    public ExternalCustomer Customer { get; private set; } = null!;

    /// <summary>
    /// Snapshot reference of the branch at the time of sale.
    /// </summary>
    public ExternalBranch Branch { get; private set; } = null!;

    /// <summary>
    /// Gets the collection of items included in the sale.
    /// </summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets the total monetary amount of the sale.
    /// This value is calculated based on the items.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets the current status of the sale.
    /// Possible values: Active or Cancelled.
    /// </summary>
    public SaleStatus Status { get; private set; }

    private Sale() { } // Required by EF Core

    public Sale(
        string saleNumber,
        ExternalCustomer customer,
        ExternalBranch branch)
    {
        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Branch = branch ?? throw new ArgumentNullException(nameof(branch));
        Date = DateTimeOffset.UtcNow;
        Status = SaleStatus.Active;
    }

    public void AddItem(SaleItem item)
    {
        if (Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot add items to a cancelled sale.");
        }

        _items.Add(item);
        RecalculateTotal();
    }

    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException("Sale already cancelled.");
        }

        Status = SaleStatus.Cancelled;
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.TotalAmount);
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }

}