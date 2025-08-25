using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.SaleItems.GetBySaleItemId;

/// <summary>
/// Handler for processing GetBySaleItemIdQuery requests
/// </summary>
public class GetBySaleItemIdHandler : IRequestHandler<GetBySaleItemIdQuery, GetBySaleItemIdResult>
{
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetBySaleItemIdHandler
    /// </summary>
    /// <param name="saleItemRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for GetBySaleItemsIdCommand</param>
    public GetBySaleItemIdHandler(
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
    /// <returns>The sale details if found</returns>
    public async Task<GetBySaleItemIdResult> Handle(GetBySaleItemIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetBySaleItemItemIdValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var saleItem = await _saleItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (saleItem == null)
        {
            throw new KeyNotFoundException($"Sale Item with ID {request.Id} not found");
        }

        return _mapper.Map<GetBySaleItemIdResult>(saleItem);
    }
}
