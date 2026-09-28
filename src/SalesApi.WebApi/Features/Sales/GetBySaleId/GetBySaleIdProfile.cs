using AutoMapper;
using SalesApi.Application.Sales.GetBySaleId;

namespace SalesApi.WebApi.Features.Sales.GetBySaleId;

/// <summary>
/// Maps the Application-layer result (already flat) to the API response.
/// Field names match exactly, so no .ForMember is needed here.
/// </summary>
public class GetBySaleIdProfile : Profile
{
    public GetBySaleIdProfile()
    {
        CreateMap<GetBySaleIdResult, SaleDetailResponse>();
        CreateMap<GetBySaleIdItemResult, SaleDetailItemResponse>();
    }
}
