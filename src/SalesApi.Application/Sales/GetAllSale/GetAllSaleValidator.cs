using FluentValidation;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Validator for GetAllSaleQuery
/// </summary>
public class GetAllSaleValidator : AbstractValidator<GetAllSaleQuery>
{
    public GetAllSaleValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.MinTotalAmount.HasValue || !x.MaxTotalAmount.HasValue || x.MinTotalAmount <= x.MaxTotalAmount)
            .WithMessage("_minTotalAmount must not be greater than _maxTotalAmount.");

        RuleFor(x => x)
            .Must(x => !x.MinDate.HasValue || !x.MaxDate.HasValue || x.MinDate <= x.MaxDate)
            .WithMessage("_minDate must not be after _maxDate.");
    }
}
