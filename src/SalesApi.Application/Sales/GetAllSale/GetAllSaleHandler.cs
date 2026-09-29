using FluentValidation;
using MediatR;
using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Handler for processing GetAllSaleQuery requests: filters and orders sales
/// in memory, since ISaleRepository.GetAllAsync already materializes everything.
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

        var sales = await _saleRepository.GetAllAsync(cancellationToken) ?? new List<Sale>();
        IEnumerable<Sale> filtered = sales;

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            filtered = ApplyStringFilter(filtered, s => s.Customer.Name, request.CustomerName);

        if (!string.IsNullOrWhiteSpace(request.Status))
            filtered = filtered.Where(s => string.Equals(s.Status.ToString(), request.Status, StringComparison.OrdinalIgnoreCase));

        if (request.MinTotalAmount.HasValue)
            filtered = filtered.Where(s => s.TotalAmount >= request.MinTotalAmount.Value);

        if (request.MaxTotalAmount.HasValue)
            filtered = filtered.Where(s => s.TotalAmount <= request.MaxTotalAmount.Value);

        if (request.MinDate.HasValue)
            filtered = filtered.Where(s => s.Date.Date >= request.MinDate.Value.Date);

        if (request.MaxDate.HasValue)
            filtered = filtered.Where(s => s.Date.Date <= request.MaxDate.Value.Date);

        return new GetAllSaleResult { Data = ApplyOrder(filtered, request.Order).ToList() };
    }

    /// <summary>
    /// Matches the API's convention: "value*" (starts with), "*value"
    /// (ends with), "*value*" (contains), or exact match with no asterisk.
    /// </summary>
    private static IEnumerable<Sale> ApplyStringFilter(IEnumerable<Sale> source, Func<Sale, string> selector, string filter)
    {
        if (filter.Length > 1 && filter.StartsWith('*') && filter.EndsWith('*'))
        {
            var value = filter.Trim('*');
            return source.Where(s => selector(s).Contains(value, StringComparison.OrdinalIgnoreCase));
        }
        if (filter.StartsWith('*'))
        {
            var value = filter.TrimStart('*');
            return source.Where(s => selector(s).EndsWith(value, StringComparison.OrdinalIgnoreCase));
        }
        if (filter.EndsWith('*'))
        {
            var value = filter.TrimEnd('*');
            return source.Where(s => selector(s).StartsWith(value, StringComparison.OrdinalIgnoreCase));
        }
        return source.Where(s => string.Equals(selector(s), filter, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Parses "field desc, field2, field3 asc" - comma-separated fields,
    /// each optionally followed by asc/desc (default asc), matching
    /// the API's _order example.
    /// </summary>
    private static IEnumerable<Sale> ApplyOrder(IEnumerable<Sale> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return source.OrderBy(s => s.SaleNumber);

        IOrderedEnumerable<Sale>? ordered = null;

        foreach (var clause in order.Trim('"').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = parts[0];
            var descending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            Func<Sale, object> keySelector = field.ToLowerInvariant() switch
            {
                "salenumber" => s => s.SaleNumber,
                "totalamount" => s => s.TotalAmount,
                "date" => s => s.Date,
                "status" => s => s.Status.ToString(),
                "customername" => s => s.Customer.Name,
                "branchname" => s => s.Branch.Name,
                _ => s => s.SaleNumber
            };

            ordered = ordered is null
                ? (descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector))
                : (descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector));
        }

        return ordered ?? source;
    }
}
