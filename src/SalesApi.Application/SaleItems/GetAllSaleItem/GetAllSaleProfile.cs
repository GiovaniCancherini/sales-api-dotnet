using SalesApi.Domain.Entities;
using AutoMapper;

namespace SalesApi.Application.SaleItems.GetAllSaleItem;

/// <summary>
/// Profile for mapping between User entity and GetAllSaleItemResult
/// </summary>
public class GetAllSaleItemProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetAllSaleItem operation
    /// </summary>
    public GetAllSaleItemProfile()
    {
        CreateMap<SaleItem, GetAllSaleItemResult>();
    }
}
