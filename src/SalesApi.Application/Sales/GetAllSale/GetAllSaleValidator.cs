using SalesApi.Domain.Enums;
using FluentValidation;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Validator for GetAllSaleQuery
/// </summary>
public class GetAllSaleValidator : AbstractValidator<GetAllSaleQuery>
{
    public GetAllSaleValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("_page must be at least 1.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100).WithMessage("_size must be between 1 and 100.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status)
                || status.Equals(nameof(SaleStatus.Active), StringComparison.OrdinalIgnoreCase)
                || status.Equals(nameof(SaleStatus.Cancelled), StringComparison.OrdinalIgnoreCase))
            .WithMessage("status must be Active or Cancelled.");

        RuleFor(x => x)
            .Must(x => !x.MinTotalAmount.HasValue || !x.MaxTotalAmount.HasValue || x.MinTotalAmount <= x.MaxTotalAmount)
            .WithMessage("_minTotalAmount must not be greater than _maxTotalAmount.");

        RuleFor(x => x)
            .Must(x => !x.MinDate.HasValue || !x.MaxDate.HasValue || x.MinDate <= x.MaxDate)
            .WithMessage("_minDate must not be after _maxDate.");
    }
}
