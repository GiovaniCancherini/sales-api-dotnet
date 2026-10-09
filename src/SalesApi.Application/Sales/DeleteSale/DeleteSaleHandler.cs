using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.DeleteSale;

/// <summary>
/// Handler for processing DeleteSaleCommand requests
/// </summary>
public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDistributedCache _cache;

    /// <summary>
    /// Initializes a new instance of DeleteSaleHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="cache">The distributed cache, invalidated after the delete</param>
    public DeleteSaleHandler(
        ISaleRepository saleRepository,
        IDistributedCache cache)
    {
        _saleRepository = saleRepository;
        _cache = cache;
    }

    /// <summary>
    /// Handles the DeleteSaleCommand request
    /// </summary>
    /// <param name="request">The DeleteSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the delete operation</returns>
    public async Task<DeleteSaleResponse> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var success = await _saleRepository.DeleteAsync(request.Id, cancellationToken);
        if (!success)
        {
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");
        }

        await _cache.RemoveAsync(SaleCacheKeys.ById(request.Id), cancellationToken);

        return new DeleteSaleResponse { Success = true };
    }
}
