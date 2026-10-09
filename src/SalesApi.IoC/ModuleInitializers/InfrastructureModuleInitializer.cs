using SalesApi.Application.Common;
using SalesApi.Domain.Repositories;
using SalesApi.ORM;
using SalesApi.ORM.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace SalesApi.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();

        // MongoClient is thread-safe and meant to be a single instance per application.
        builder.Services.AddSingleton<IMongoClient>(provider => new MongoClient(
            provider.GetRequiredService<IConfiguration>().GetConnectionString("MongoDb")
                ?? throw new InvalidOperationException("ConnectionStrings:MongoDb is not configured.")));
        builder.Services.AddSingleton<ISaleEventStore, MongoSaleEventStore>();

        // Exposes Redis as IDistributedCache, so handlers depend on the abstraction, not on Redis.
        // The resilient decorator keeps the cache optional: a Redis outage never fails a request.
        builder.Services.Configure<RedisCacheOptions>(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");
            options.InstanceName = "salesapi:";
        });
        builder.Services.AddSingleton<IDistributedCache>(provider => new ResilientDistributedCache(
            new RedisCache(provider.GetRequiredService<IOptions<RedisCacheOptions>>()),
            provider.GetRequiredService<ILogger<ResilientDistributedCache>>()));
    }
}