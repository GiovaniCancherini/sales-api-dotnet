using SalesApi.Common.Validation;
using SalesApi.WebApi.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace SalesApi.WebApi.Middleware
{
    /// <summary>
    /// Translates exceptions thrown by Handlers (and by EF/Postgres) into proper
    /// HTTP status codes and the standard ApiResponse shape, instead of letting
    /// them fall through to the default dev exception page (raw stack trace, 500,
    /// and outside Development an empty unhelpful 500).
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest, "Validation Failed",
                    ex.Errors.Select(error => (ValidationErrorDetail)error));
            }
            catch (KeyNotFoundException ex)
            {
                // Handlers throw this for "Sale/User/SaleItem with ID X not found".
                await WriteResponseAsync(context, StatusCodes.Status404NotFound, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                // AuthenticateUserHandler: invalid credentials / inactive user.
                await WriteResponseAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // CreateUserHandler: "User with email X already exists" and similar
                // business-rule conflicts a Handler already checked for explicitly.
                await WriteResponseAsync(context, StatusCodes.Status409Conflict, ex.Message);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                // Unique constraint violated at the database (e.g. duplicate SaleNumber)
                // that no Handler checked for up front. Message is generic on purpose —
                // never echo raw SQL/constraint details back to the client.
                await WriteResponseAsync(context, StatusCodes.Status409Conflict,
                    "A record with the same unique value already exists.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                await WriteResponseAsync(context, StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        private static Task WriteResponseAsync(
            HttpContext context,
            int statusCode,
            string message,
            IEnumerable<ValidationErrorDetail>? errors = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? []
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
