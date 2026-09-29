using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace SalesApi.Common.Security
{
    public static class AuthenticationExtension
    {
        // Same {type, error, detail} shape as WebApi's ApiErrorResponse.
        // Can't reference that type directly - Common must not depend on WebApi.
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            var secretKey = configuration["Jwt:SecretKey"]?.ToString();
            ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                // Without this, a missing/invalid/expired token produces ASP.NET's
                // default empty-body 401/403 - the only error responses in the API
                // that wouldn't follow the API's error format, since they
                // never reach ExceptionHandlingMiddleware's catch blocks (the
                // authentication/authorization middleware short-circuits the
                // pipeline directly, it doesn't throw).
                x.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return WriteErrorAsync(context.Response, StatusCodes.Status401Unauthorized,
                            "AuthenticationError", "Authentication failed", "A valid authentication token is required.");
                    },
                    OnForbidden = context =>
                        WriteErrorAsync(context.Response, StatusCodes.Status403Forbidden,
                            "AuthorizationError", "Access denied", "You do not have permission to access this resource.")
                };
            });

            return services;
        }

        private static Task WriteErrorAsync(HttpResponse response, int statusCode, string type, string error, string detail)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new { type, error, detail }, JsonOptions);
            return response.WriteAsync(body);
        }
    }
}
