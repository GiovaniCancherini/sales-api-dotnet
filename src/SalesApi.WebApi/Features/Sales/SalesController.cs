using SalesApi.Application.Sales.AddSaleItem;
using SalesApi.Application.Sales.CancelSale;
using SalesApi.Application.Sales.CancelSaleItem;
using SalesApi.Application.Sales.CreateSale;
using SalesApi.Application.Sales.DeleteSale;
using SalesApi.Application.Sales.GetAllSale;
using SalesApi.Application.Sales.GetBySaleId;
using SalesApi.Application.Sales.UpdateSale;
using SalesApi.WebApi.Common;
using SalesApi.WebApi.Features.Sales.AddSaleItem;
using SalesApi.WebApi.Features.Sales.CreateSale;
using SalesApi.WebApi.Features.Sales.GetAllSale;
using SalesApi.WebApi.Features.Sales.GetBySaleId;
using SalesApi.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SalesApi.WebApi.Features.Sales;

/// <summary>
/// Controller for managing sale operations
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale
    /// </summary>
    /// <param name="request">The sale creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateSaleCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = _mapper.Map<CreateSaleResponse>(response)
        });
    }

    /// <summary>
    /// Retrieves the full detail of a sale, including its items.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSaleById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBySaleIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(_mapper.Map<SaleDetailResponse>(result), "Sale retrieved successfully");
    }

    /// <summary>
    /// Replaces the full item list of an existing sale (correcting a sale
    /// after creation). Customer/Branch/Status aren't editable here - see
    /// UpdateSaleCommand's remarks for why.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        await _mediator.Send(command, cancellationToken);

        return Ok("Sale updated successfully");
    }

    /// <summary>
    /// Deletes a sale.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);

        return Ok("Sale deleted successfully");
    }

    /// <summary>
    /// Adds one item to an existing sale.
    /// </summary>
    [HttpPost("{id}/items")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddSaleItem([FromRoute] Guid id, [FromBody] AddSaleItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddSaleItemCommand
        {
            SaleId = id,
            Item = _mapper.Map<CreateSaleItemCommand>(request)
        };

        await _mediator.Send(command, cancellationToken);

        return Ok("Item added successfully");
    }

    /// <summary>
    /// Cancels a sale.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelSaleCommand(id), cancellationToken);

        return Ok("Sale cancelled successfully");
    }

    /// <summary>
    /// Cancels one item of an existing sale.
    /// </summary>
    [HttpDelete("{id}/items/{itemId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSaleItem([FromRoute] Guid id, [FromRoute] Guid itemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelSaleItemCommand(id, itemId), cancellationToken);

        return Ok("Item cancelled successfully");
    }

    /// <summary>
    /// Lists sales, with pagination (_page/_size), ordering (_order) and
    /// filtering (customerName, status, _minTotalAmount/_maxTotalAmount,
    /// _minDate/_maxDate). See the README.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SaleListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListSales([FromQuery] ListSalesRequest request, CancellationToken cancellationToken)
    {
        var query = new GetAllSaleQuery
        {
            Page = request.Page,
            Size = request.Size,
            Order = request.Order,
            CustomerName = request.CustomerName,
            Status = request.Status,
            MinTotalAmount = request.MinTotalAmount,
            MaxTotalAmount = request.MaxTotalAmount,
            MinDate = request.MinDate,
            MaxDate = request.MaxDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        var items = _mapper.Map<List<SaleListItemResponse>>(result.Data);
        var paginated = new PaginatedList<SaleListItemResponse>(items, result.TotalCount, request.Page, request.Size);

        return OkPaginated(paginated, "Sales retrieved successfully");
    }
}
