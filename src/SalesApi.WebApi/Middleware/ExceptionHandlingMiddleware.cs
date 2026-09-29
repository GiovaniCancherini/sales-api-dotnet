using SalesApi.WebApi.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace SalesApi.WebApi.Middleware
{
    /// <summary>
    /// Translates exceptions thrown by Handlers (and by EF/Postgres) into proper
    /// HTTP status codes and the ApiErrorResponse shape documented in the README
    /// ({type, error, detail}), instead of letting them fall through to the
    /// default dev exception page (raw stack trace, 500, and outside
    /// Development an empty unhelpful 500).
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
                var detail = string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
                await WriteResponseAsync(context, StatusCodes.Status400BadRequest,
                    "ValidationError", "Invalid input data", detail);
            }
            catch (KeyNotFoundException ex)
            {
                // Handlers throw this for "Sale/User/SaleItem with ID X not found".
                await WriteResponseAsync(context, StatusCodes.Status404NotFound,
                    "ResourceNotFound", "Resource not found", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                // AuthenticateUserHandler: invalid credentials / inactive user.
                await WriteResponseAsync(context, StatusCodes.Status401Unauthorized,
                    "AuthenticationError", "Authentication failed", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // CreateUserHandler: "User with email X already exists" and similar
                // business-rule conflicts a Handler already checked for explicitly.
                await WriteResponseAsync(context, StatusCodes.Status409Conflict,
                    "Conflict", "The request conflicts with the current state of the resource", ex.Message);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                // Unique constraint violated at the database (e.g. duplicate SaleNumber)
                // that no Handler checked for up front. Detail is generic on purpose:
                // never echo raw SQL/constraint details back to the client.
                await WriteResponseAsync(context, StatusCodes.Status409Conflict,
                    "Conflict", "The request conflicts with the current state of the resource",
                    "A record with the same unique value already exists.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                await WriteResponseAsync(context, StatusCodes.Status500InternalServerError,
                    "ServerError", "An unexpected error occurred", "An unexpected error occurred.");
            }
        }

        private static Task WriteResponseAsync(
            HttpContext context,
            int statusCode,
            string type,
            string error,
            string detail)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiErrorResponse
            {
                Type = type,
                Error = error,
                Detail = detail
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
