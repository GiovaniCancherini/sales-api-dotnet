using SalesApi.Application;
using SalesApi.Common.HealthChecks;
using SalesApi.Common.Logging;
using SalesApi.Common.Security;
using SalesApi.Common.Validation;
using SalesApi.IoC;
using SalesApi.ORM;
using SalesApi.WebApi.Middleware;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;

namespace SalesApi.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            builder.Services.AddSwaggerGen(options =>
            {
                // Adds the "Authorize" lock button: without this, Swagger UI has
                // no way to attach a Bearer token to a request, even though
                // AddJwtAuthentication + UseAuthentication are already wired up.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the token returned by POST /api/Auth (no need to type \"Bearer \" first)."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("SalesApi.ORM")
                )
            );

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.RegisterDependencies();

            // AppDomain.CurrentDomain.GetAssemblies() só enxerga assemblies já carregados
            // na memória neste exato instante - nada antes desta linha força o assembly
            // Application a carregar, então os Profiles de lá (ex.: CreateSaleProfile)
            // ficam de fora silenciosamente. Aponta explícito pros assemblies que importam.
            builder.Services.AddAutoMapper(
                (System.Action<AutoMapper.IMapperConfigurationExpression>)null,
                typeof(ApplicationLayer).Assembly,
                typeof(Program).Assembly
            );
                
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            var app = builder.Build();

            // Postgres runs without a volume in docker-compose: every container
            // recreation wipes the data and comes back with an empty schema. Apply
            // pending migrations automatically on startup instead of requiring a
            // manual "dotnet ef database update" every time (including for anyone
            // running the project from scratch).
            using (var scope = app.Services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<DefaultContext>().Database.Migrate();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
