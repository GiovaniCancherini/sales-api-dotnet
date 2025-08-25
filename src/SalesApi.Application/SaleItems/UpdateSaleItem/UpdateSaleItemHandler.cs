using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SalesApi.Application.SaleItems.UpdateSaleItem;

/// <summary>
/// Handler for processing UpdateSaleItemCommand requests
/// </summary>
public class UpdateSaleItemHandler : IRequestHandler<UpdateSaleItemCommand, UpdateSaleItemResponse>
{
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of UpdateSaleItemHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for CreateSaleItemCommand</param>
    public UpdateSaleItemHandler(ISaleItemRepository saleRepository, IMapper mapper)
    {
        _saleItemRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the UpdateSaleItemCommand request
    /// </summary>
    /// <param name="request">The UpdateSaleItem command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the delete operation</returns>
    public async Task<UpdateSaleItemResponse> Handle(UpdateSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = _mapper.Map<SaleItem>(request);

        var updatedSaleItem = await _saleItemRepository.UpdateAsync(sale, cancellationToken);
        var result = _mapper.Map<UpdateSaleItemResponse>(updatedSaleItem);
        
        return result;
    }
}
