using SalesApi.Domain.Entities;
using AutoMapper;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Profile for mapping between Sale (+ its SaleItems) and GetBySaleIdResult.
/// </summary>
public class GetBySaleIdProfile : Profile
{
    public GetBySaleIdProfile()
    {
        CreateMap<Sale, GetBySaleIdResult>()
            .ForMember(d => d.CustomerId, opt => opt.MapFrom(s => s.Customer.Id))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.BranchId, opt => opt.MapFrom(s => s.Branch.Id))
            .ForMember(d => d.BranchName, opt => opt.MapFrom(s => s.Branch.Name))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<SaleItem, GetBySaleIdItemResult>()
            .ForMember(d => d.ProductId, opt => opt.MapFrom(s => s.Product.Id))
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductSku, opt => opt.MapFrom(s => s.Product.SKU));
    }
}
