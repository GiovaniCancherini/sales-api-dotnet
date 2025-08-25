using SalesApi.Domain.Entities;
using AutoMapper;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Profile for mapping between User entity and GetBySaleIdResult
/// </summary>
public class GetBySaleIdProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetBySaleId operation
    /// </summary>
    public GetBySaleIdProfile()
    {
        CreateMap<Sale, GetBySaleIdResult>();
    }
}
