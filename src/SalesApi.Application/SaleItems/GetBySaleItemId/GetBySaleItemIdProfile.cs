using SalesApi.Domain.Entities;
using AutoMapper;

namespace SalesApi.Application.SaleItems.GetBySaleItemId;

/// <summary>
/// Profile for mapping between SaleItem entity and GetBySaleItemIdResult
/// </summary>
public class GetBySaleItemIdProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetBySaleItemId operation
    /// </summary>
    public GetBySaleItemIdProfile()
    {
        CreateMap<SaleItem, GetBySaleItemIdResult>();
    }
}
