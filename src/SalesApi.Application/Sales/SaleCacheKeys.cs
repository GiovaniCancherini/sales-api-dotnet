namespace SalesApi.Application.Sales;

/// <summary>
/// Single source of the sale cache keys, shared by the read path (GetBySaleId)
/// and every write path that has to invalidate it.
/// </summary>
public static class SaleCacheKeys
{
    public static string ById(Guid id) => $"sale:{id}";
}
