using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.SaleItems.GetAllSaleItem;

/// <summary>
/// Handler for processing GetAllSaleItemCommand requests
/// </summary>
public class GetAllSaleItemHandler : IRequestHandler<GetAllSaleItemQuery, GetAllSaleItemResult>
{
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetAllSaleItemHandler
    /// </summary>
    /// <param name="saleItemRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for GetAllSaleItemCommand</param>
    public GetAllSaleItemHandler(
        ISaleItemRepository saleItemRepository,
        IMapper mapper)
    {
        _saleItemRepository = saleItemRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetAllSaleItemCommand request
    /// </summary>
    /// <param name="request">The GetAllSaleItem command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale details if found</returns>
    public async Task<GetAllSaleItemResult> Handle(GetAllSaleItemQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAllSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = await _saleItemRepository.GetAllAsync(cancellationToken);
        
        return _mapper.Map<GetAllSaleItemResult>(sale);
    }
}
