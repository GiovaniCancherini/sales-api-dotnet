using AutoMapper;
using SalesApi.Domain.Entities;

namespace SalesApi.Application.Sales.CreateSale;

/// <summary>
/// Profile for mapping between Sale entity and CreateSaleResponse
/// </summary>
public class CreateSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CreateSale operation
    /// </summary>
    public CreateSaleProfile()
    {
        // No Command -> Sale map: Sale only comes into existence via
        // Sale.Create(...) (private constructor), built manually in the handler.
        CreateMap<Sale, CreateSaleResult>();
    }
}
