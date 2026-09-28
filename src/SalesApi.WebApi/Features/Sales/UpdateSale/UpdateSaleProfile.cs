using AutoMapper;
using SalesApi.Application.Sales.UpdateSale;

namespace SalesApi.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        // Id isn't part of the request body (comes from the route) - the
        // controller sets it on the mapped command explicitly.
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
    }
}
