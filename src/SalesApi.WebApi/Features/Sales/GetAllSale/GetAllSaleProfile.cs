using AutoMapper;
using SalesApi.Domain.Entities;

namespace SalesApi.WebApi.Features.Sales.GetAllSale;

/// <summary>
/// Maps the Sale aggregate down to the flat list response. Safe for AutoMapper:
/// this only reads public getters, it never has to construct a Sale.
/// </summary>
public class GetAllSaleProfile : Profile
{
    public GetAllSaleProfile()
    {
        CreateMap<Sale, SaleListItemResponse>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.BranchName, opt => opt.MapFrom(s => s.Branch.Name))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
