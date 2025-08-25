using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.SaleItems.DeleteSaleItem;

/// <summary>
/// Handler for processing DeleteSaleItemCommand requests
/// </summary>
public class DeleteSaleItemHandler : IRequestHandler<DeleteSaleItemCommand, DeleteSaleItemResponse>
{
    private readonly ISaleItemRepository _saleRepository;

    /// <summary>
    /// Initializes a new instance of DeleteSaleItemHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="validator">The validator for DeleteSaleItemCommand</param>
    public DeleteSaleItemHandler(
        ISaleItemRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    /// <summary>
    /// Handles the DeleteSaleItemCommand request
    /// </summary>
    /// <param name="request">The DeleteSaleItem command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the delete operation</returns>
    public async Task<DeleteSaleItemResponse> Handle(DeleteSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var success = await _saleRepository.DeleteAsync(request.Id, cancellationToken);
        if (!success)
        {
            throw new KeyNotFoundException($"Sale Item with ID {request.Id} not found");
        }

        return new DeleteSaleItemResponse { Success = true };
    }
}
