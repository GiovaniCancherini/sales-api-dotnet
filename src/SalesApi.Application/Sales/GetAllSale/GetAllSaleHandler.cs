using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.GetAllSale;

/// <summary>
/// Handler for processing GetAllSaleCommand requests
/// </summary>
public class GetAllSaleHandler : IRequestHandler<GetAllSaleQuery, GetAllSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetAllSaleHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for GetAllSaleCommand</param>
    public GetAllSaleHandler(
        ISaleRepository saleRepository,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetAllSaleCommand request
    /// </summary>
    /// <param name="request">The GetAllSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale details if found</returns>
    public async Task<GetAllSaleResult> Handle(GetAllSaleQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAllSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = await _saleRepository.GetAllAsync(cancellationToken);
        
        return _mapper.Map<GetAllSaleResult>(sale);
    }
}
