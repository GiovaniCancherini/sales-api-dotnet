using AutoMapper;
using SalesApi.Application.Sales.CreateSale;

namespace SalesApi.WebApi.Features.Sales.AddSaleItem;

public class AddSaleItemProfile : Profile
{
    public AddSaleItemProfile()
    {
        // SaleId isn't part of the request body (comes from the route) - the
        // controller sets it on the mapped command explicitly.
        CreateMap<AddSaleItemRequest, CreateSaleItemCommand>();
    }
}
