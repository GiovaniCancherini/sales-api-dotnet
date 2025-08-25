using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.SaleItems.GetByIdSale;

/// <summary>
/// Handler for processing GetBySaleItemIdCommand requests
/// </summary>
public class GetByIdSaleHandler : IRequestHandler<GetByIdSaleIdQuery, GetByIdSaleResult>
{
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetByIdSaleHandler
    /// </summary>
    /// <param name="saleItemRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for GetBySaleItemIdCommand</param>
    public GetByIdSaleHandler(
        ISaleItemRepository saleItemRepository,
        IMapper mapper)
    {
        _saleItemRepository = saleItemRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetBySaleItemIdCommand request
    /// </summary>
    /// <param name="request">The GetBySaleItemId command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale item details if found</returns>
    public async Task<GetByIdSaleResult> Handle(GetByIdSaleIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetByIdSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var saleItem = await _saleItemRepository.GetAllByIdSaleAsync(request.IdSale, cancellationToken);
        if (saleItem == null)
        {
            throw new KeyNotFoundException($"SaleItem with ID Sale {request.IdSale} not found");
        }

        return _mapper.Map<GetByIdSaleResult>(saleItem);
    }
}
