using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace SalesApi.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult Ok<T>(T data) =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true });

    // Overload for a custom success message. Without this, callers that needed
    // one (AuthController, several UsersController actions) built their own
    // ApiResponseWithData and passed it into Ok(...) - which matched THIS same
    // Ok<T>(T data) by generic inference and wrapped it a second time.
    protected IActionResult Ok<T>(T data, string message) =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    // For a success response with no data payload (e.g. after a delete),
    // mirroring the existing BadRequest(string)/NotFound(string) shape below.
    // A non-generic overload wins over Ok<T>(T data) when called with a
    // string literal, so this doesn't collide with it.
    protected IActionResult Ok(string message) =>
            base.Ok(new ApiResponse { Message = message, Success = true });

    protected IActionResult BadRequest(string message) =>
        base.BadRequest(new ApiErrorResponse { Type = "ValidationError", Error = "Invalid input data", Detail = message });

    // Matches callers that pass a FluentValidation ValidationResult.Errors directly
    // (e.g. `return BadRequest(validationResult.Errors);`) - without this overload,
    // those calls bind to ControllerBase's own BadRequest(object), returning the
    // raw ValidationFailure list instead of the standard error shape.
    protected IActionResult BadRequest(IEnumerable<ValidationFailure> errors) =>
        base.BadRequest(new ApiErrorResponse
        {
            Type = "ValidationError",
            Error = "Invalid input data",
            Detail = string.Join(" ", errors.Select(e => e.ErrorMessage))
        });

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(new ApiErrorResponse { Type = "ResourceNotFound", Error = "Resource not found", Detail = message });

    protected IActionResult OkPaginated<T>(PaginatedList<T> pagedList, string message = "") =>
            // base.Ok, not Ok(...): PaginatedResponse<T> already IS an
            // ApiResponseWithData<T>, so the local Ok<T>(T data) overload
            // above would wrap it a second time.
            base.Ok(new PaginatedResponse<T>
            {
                Data = pagedList,
                CurrentPage = pagedList.CurrentPage,
                TotalPages = pagedList.TotalPages,
                TotalCount = pagedList.TotalCount,
                Success = true,
                Message = message
            });
}
