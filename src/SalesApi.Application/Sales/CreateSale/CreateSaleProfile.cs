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
        // Sale só nasce via Sale.Create(...) (construtor privado + invariantes),
        // então não existe mapeamento AutoMapper de Command -> Sale.
        // A construção acontece manualmente no CreateSaleHandler.
        CreateMap<Sale, CreateSaleResult>();
    }
}
