using FluentValidation;
using MediatR;
using SalesApi.Application.Common;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Enums;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Handler for processing GetAllSaleQuery requests: filtering, ordering and
/// pagination are composed as a query and run in the database.
/// </summary>
public class GetAllSaleHandler : IRequestHandler<GetAllSaleQuery, GetAllSaleResult>
{
    private readonly ISaleRepository _saleRepository;

    public GetAllSaleHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<GetAllSaleResult> Handle(GetAllSaleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAllSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var (sales, totalCount) = await _saleRepository.ListAsync(
            query => ApplyOrder(ApplyFilters(query, request), request.Order),
            request.Page, request.Size, cancellationToken);

        return new GetAllSaleResult { Data = sales, TotalCount = totalCount };
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, GetAllSaleQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            query = ApplyCustomerNameFilter(query, request.CustomerName);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = Enum.Parse<SaleStatus>(request.Status, ignoreCase: true);
            query = query.Where(s => s.Status == status);
        }

        if (request.MinTotalAmount.HasValue)
        {
            var min = request.MinTotalAmount.Value;
            query = query.Where(s => s.TotalAmount >= min);
        }

        if (request.MaxTotalAmount.HasValue)
        {
            var max = request.MaxTotalAmount.Value;
            query = query.Where(s => s.TotalAmount <= max);
        }

        // Whole UTC days: _minDate from midnight, _maxDate up to (not including) the next midnight.
        if (request.MinDate.HasValue)
        {
            var from = UtcMidnight(request.MinDate.Value);
            query = query.Where(s => s.Date >= from);
        }

        if (request.MaxDate.HasValue)
        {
            var until = UtcMidnight(request.MaxDate.Value).AddDays(1);
            query = query.Where(s => s.Date < until);
        }

        return query;
    }

    /// <summary>
    /// Matches the API's convention, case-insensitively: "value*" (starts with),
    /// "*value" (ends with), "*value*" (contains), or exact match with no asterisk.
    /// </summary>
    private static IQueryable<Sale> ApplyCustomerNameFilter(IQueryable<Sale> query, string filter)
    {
        var value = filter.Trim('*').ToLowerInvariant();

        return (filter.StartsWith('*'), filter.EndsWith('*')) switch
        {
            (true, true) => query.Where(s => s.Customer.Name.ToLower().Contains(value)),
            (true, false) => query.Where(s => s.Customer.Name.ToLower().EndsWith(value)),
            (false, true) => query.Where(s => s.Customer.Name.ToLower().StartsWith(value)),
            _ => query.Where(s => s.Customer.Name.ToLower() == value)
        };
    }

    private static IQueryable<Sale> ApplyOrder(IQueryable<Sale> query, string? order)
    {
        IOrderedQueryable<Sale>? ordered = null;

        foreach (var (field, descending) in QueryOrdering.Parse(order))
        {
            ordered = field switch
            {
                "totalamount" => QueryOrdering.Then(query, ordered, s => s.TotalAmount, descending),
                "date" => QueryOrdering.Then(query, ordered, s => s.Date, descending),
                "status" => QueryOrdering.Then(query, ordered, s => s.Status, descending),
                "customername" => QueryOrdering.Then(query, ordered, s => s.Customer.Name, descending),
                "branchname" => QueryOrdering.Then(query, ordered, s => s.Branch.Name, descending),
                _ => QueryOrdering.Then(query, ordered, s => s.SaleNumber, descending)
            };
        }

        // SaleNumber is unique, so it also breaks ties and keeps pages stable.
        return ordered?.ThenBy(s => s.SaleNumber) ?? query.OrderBy(s => s.SaleNumber);
    }

    private static DateTimeOffset UtcMidnight(DateTime date) =>
        new(DateTime.SpecifyKind(date.Date, DateTimeKind.Utc));
}
