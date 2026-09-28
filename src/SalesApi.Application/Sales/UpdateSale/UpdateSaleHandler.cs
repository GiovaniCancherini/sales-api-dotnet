using SalesApi.Domain.Entities;
using SalesApi.Domain.Repositories;
using SalesApi.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace SalesApi.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests. Loads the Sale, builds
/// SaleItem entities directly from the command's flat data (no AutoMapper -
/// same construction issue CreateSaleHandler had would apply here too, since
/// SaleItem only exposes a constructor + private setters), then replaces the
/// item list via Sale.ReplaceItems.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResponse>
{
    private readonly ISaleRepository _saleRepository;

    public UpdateSaleHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<UpdateSaleResponse> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        var newItems = request.Items.Select(item =>
        {
            var product = new ExternalProduct(Guid.Parse(item.ProductId), item.ProductName, item.ProductSku);
            return new SaleItem(product, item.Quantity, item.UnitPrice);
        }).ToList();

        sale.ReplaceItems(newItems);

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        return new UpdateSaleResponse { Success = true };
    }
}
