using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;
using SalesApi.Domain.Entities;
using SalesApi.Common.Security;

namespace SalesApi.Application.SaleItems.AddSaleItem;

/// <summary>
/// Handler for processing AddSaleItemCommand requests
/// </summary>
public class AddSaleItemHandler : IRequestHandler<AddSaleItemCommand, AddSaleItemResult>
{
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of AddSaleItemHandler
    /// </summary>
    /// <param name="saleItemRepository">The saleItem repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for AddSaleItemCommand</param>
    public AddSaleItemHandler(ISaleItemRepository saleItemRepository, IMapper mapper)
    {
        _saleItemRepository = saleItemRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the AddSaleItemCommand request
    /// </summary>
    /// <param name="command">The AddSaleItem command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created saleItem details</returns>
    public async Task<AddSaleItemResult> Handle(AddSaleItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new AddSaleItemCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var saleItem = _mapper.Map<SaleItem>(command);

        var createdSaleItem = await _saleItemRepository.AddAsync(saleItem, cancellationToken);
        var result = _mapper.Map<AddSaleItemResult>(createdSaleItem);
        return result;
    }
}
