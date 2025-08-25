using AutoMapper;
using MediatR;
using FluentValidation;
using SalesApi.Domain.Repositories;

namespace SalesApi.Application.Sales.GetBySaleId;

/// <summary>
/// Handler for processing GetBySaleIdCommand requests
/// </summary>
public class GetBySaleIdHandler : IRequestHandler<GetBySaleIdCommand, GetBySaleIdResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetBySaleIdHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="validator">The validator for GetBySaleIdCommand</param>
    public GetBySaleIdHandler(
        ISaleRepository saleRepository,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetBySaleIdCommand request
    /// </summary>
    /// <param name="request">The GetBySaleId command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale details if found</returns>
    public async Task<GetBySaleIdResult> Handle(GetBySaleIdCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetBySaleIdValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");
        }

        return _mapper.Map<GetBySaleIdResult>(sale);
    }
}
