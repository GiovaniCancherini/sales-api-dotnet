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

    // For a success response with no data payload (e.g. after a delete).
    // A non-generic overload wins over Ok<T>(T data) when called with a
    // string literal, so this doesn't collide with it.
    // Error responses don't live here: ExceptionHandlingMiddleware turns
    // validation, not-found and conflict exceptions into the standard error shape.
    protected IActionResult Ok(string message) =>
            base.Ok(new ApiResponse { Message = message, Success = true });

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
