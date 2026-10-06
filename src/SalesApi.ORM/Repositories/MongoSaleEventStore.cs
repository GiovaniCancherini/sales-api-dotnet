using SalesApi.Domain.Common;
using SalesApi.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace SalesApi.ORM.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="ISaleEventStore"/>: one document per
/// event in the "sale_events" collection.
/// </summary>
public class MongoSaleEventStore : ISaleEventStore
{
    public const string DatabaseName = "sales_api";
    public const string CollectionName = "sale_events";

    private readonly IMongoCollection<SaleEventDocument> _collection;
    private readonly ILogger<MongoSaleEventStore> _logger;

    public MongoSaleEventStore(IMongoClient client, ILogger<MongoSaleEventStore> logger)
    {
        _collection = client.GetDatabase(DatabaseName).GetCollection<SaleEventDocument>(CollectionName);
        _logger = logger;
    }

    public async Task AppendAsync(DomainEvent saleEvent, Guid saleId, Guid? itemId = null, CancellationToken cancellationToken = default)
    {
        var document = new SaleEventDocument
        {
            EventType = saleEvent.GetType().Name,
            SaleId = saleId,
            ItemId = itemId,
            OccurredOn = saleEvent.OccurredOn
        };

        try
        {
            await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException)
        {
            // ponytail: best-effort. Events are published after the sale is committed in
            // Postgres, so failing the request here would report an error for a saved sale.
            // Ceiling: while MongoDB is down, each event waits serverSelectionTimeoutMS (3s)
            // before giving up, and the event is lost. An outbox table drained by a background
            // worker would remove both the wait and the loss.
            _logger.LogWarning(ex, "Could not store {EventType} for sale {SaleId} in MongoDB", document.EventType, saleId);
        }
    }
}

/// <summary>
/// One sale domain event as stored in MongoDB.
/// </summary>
public class SaleEventDocument
{
    public ObjectId Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid SaleId { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? ItemId { get; set; }

    public DateTime OccurredOn { get; set; }
}
