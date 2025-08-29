using SalesApi.Domain.Entities;
using AutoMapper;

namespace SalesApi.Application.SaleItems.GetByIdSale;

/// <summary>
/// Profile for mapping between SaleItem entity and GetBySaleItemIdResult
/// </summary>
public class GetByIdSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetBySaleItemId operation
    /// </summary>
    public GetByIdSaleProfile()
    {
        CreateMap<SaleItem, GetByIdSaleResult>();
    }
}
