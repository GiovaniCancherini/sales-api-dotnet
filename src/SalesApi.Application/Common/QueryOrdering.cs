using System.Linq.Expressions;

namespace SalesApi.Application.Common;

/// <summary>
/// the API's _order convention: "field desc, field2 asc" (asc by default),
/// built as typed expressions so EF Core can translate the ordering to SQL.
/// </summary>
internal static class QueryOrdering
{
    public static IEnumerable<(string Field, bool Descending)> Parse(string? order) =>
        (order ?? string.Empty).Trim('"')
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(clause => clause.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(parts => (parts[0].ToLowerInvariant(),
                parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)));

    public static IOrderedQueryable<T> Then<T, TKey>(
        IQueryable<T> source, IOrderedQueryable<T>? ordered, Expression<Func<T, TKey>> key, bool descending) =>
        ordered is null
            ? (descending ? source.OrderByDescending(key) : source.OrderBy(key))
            : (descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key));
}
