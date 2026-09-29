using System.Net;
using System.Net.Http.Json;
using SalesApi.WebApi.Common;
using SalesApi.WebApi.Features.Sales.CreateSale;
using SalesApi.WebApi.Features.Sales.GetBySaleId;
using SalesApi.WebApi.Features.Sales.UpdateSale;
using Xunit;

namespace SalesApi.Functional;

/// <summary>
/// End-to-end coverage of the Sales API through the real HTTP pipeline:
/// controller, validation, MediatR handler, EF Core against a real database,
/// and the response back out. Covers the same scenarios verified manually
/// via curl during development - the discount tiers, the quantity ceiling,
/// and the EF Added/Modified fix - as repeatable, automated checks.
/// </summary>
[Collection(ApiCollection.Name)]
public class SalesFunctionalTests
{
    private readonly ApiFixture _fixture;

    public SalesFunctionalTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    private static CreateSaleItemRequest ValidItem(int quantity, decimal unitPrice) => new()
    {
        ProductId = Guid.NewGuid().ToString(),
        ProductName = "Functional Test Product",
        ProductSku = "FUNC-001",
        Quantity = quantity,
        UnitPrice = unitPrice
    };

    private static CreateSaleRequest ValidSaleRequest(params CreateSaleItemRequest[] items) => new()
    {
        SaleNumber = $"FUNC-{Guid.NewGuid():N}",
        Date = DateTime.UtcNow,
        CustomerId = Guid.NewGuid().ToString(),
        CustomerName = "Functional Test Customer",
        BranchId = Guid.NewGuid().ToString(),
        BranchName = "Functional Test Branch",
        BranchCode = "FTB",
        Items = items
    };

    [Fact(DisplayName = "Given items across all discount tiers When creating a sale Then each item's discount matches the tier and the total is correct")]
    public async Task Given_ItemsAcrossDiscountTiers_When_Created_Then_DiscountsAndTotalAreCorrect()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var request = ValidSaleRequest(
            ValidItem(3, 10m),  // below 4 -> no discount: 30
            ValidItem(5, 10m),  // 4-9 -> 10%: 45
            ValidItem(15, 10m)  // 10-20 -> 20%: 120
        );

        // When
        var createResponse = await client.PostAsJsonAsync("/api/Sales", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>(JsonOptions.CaseInsensitive);

        var getResponse = await client.GetAsync($"/api/Sales/{created!.Data!.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDetailResponse>>(JsonOptions.CaseInsensitive);

        // Then
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var sale = detail!.Data!;
        Assert.Equal(195m, sale.TotalAmount); // 30 + 45 + 120

        Assert.Equal(0m, sale.Items.Single(i => i.Quantity == 3).Discount);
        Assert.Equal(5m, sale.Items.Single(i => i.Quantity == 5).Discount);
        Assert.Equal(30m, sale.Items.Single(i => i.Quantity == 15).Discount);
    }

    [Fact(DisplayName = "Given a client-supplied backdated date When creating a sale Then the persisted sale keeps that exact date")]
    public async Task Given_BackdatedDate_When_Creating_Then_PersistedDateMatches()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var request = ValidSaleRequest(ValidItem(1, 10m));
        request.Date = DateTime.UtcNow.AddDays(-3);

        // When
        var createResponse = await client.PostAsJsonAsync("/api/Sales", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>(JsonOptions.CaseInsensitive);
        var getResponse = await client.GetAsync($"/api/Sales/{created!.Data!.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDetailResponse>>(JsonOptions.CaseInsensitive);

        // Then - the date the client sent must survive, not be silently
        // replaced by the server's "now" at creation time.
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(request.Date, detail!.Data!.Date.UtcDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Given a quantity above 20 When creating a sale Then returns 400 ValidationError and the sale is not created")]
    public async Task Given_QuantityAboveTwenty_When_Creating_Then_Returns400()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var request = ValidSaleRequest(ValidItem(21, 10m));

        // When
        var response = await client.PostAsJsonAsync("/api/Sales", request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions.CaseInsensitive);
        Assert.Equal("ValidationError", body?.Type);
    }

    [Fact(DisplayName = "Given an existing sale When adding a new item Then it persists without the EF Added/Modified concurrency error")]
    public async Task Given_ExistingSale_When_AddingItem_Then_PersistsCorrectly()
    {
        // Given - this is the exact scenario that used to throw
        // DbUpdateConcurrencyException before the SaleRepository fix.
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var createResponse = await client.PostAsJsonAsync("/api/Sales", ValidSaleRequest(ValidItem(1, 10m)));
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>(JsonOptions.CaseInsensitive);

        // When
        var addItemResponse = await client.PostAsJsonAsync($"/api/Sales/{created!.Data!.Id}/items", ValidItem(1, 5m));

        // Then
        Assert.Equal(HttpStatusCode.OK, addItemResponse.StatusCode);
        var getResponse = await client.GetAsync($"/api/Sales/{created.Data.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDetailResponse>>(JsonOptions.CaseInsensitive);
        Assert.Equal(2, detail!.Data!.Items.Count);
        Assert.Equal(15m, detail.Data.TotalAmount);
    }

    [Fact(DisplayName = "Given an existing sale When replacing its items Then the old item is gone and the total reflects only the new ones")]
    public async Task Given_ExistingSale_When_ReplacingItems_Then_OldItemGoneNewTotal()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var createResponse = await client.PostAsJsonAsync("/api/Sales", ValidSaleRequest(ValidItem(1, 30m)));
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>(JsonOptions.CaseInsensitive);

        var updateRequest = new UpdateSaleRequest { Items = new List<CreateSaleItemRequest> { ValidItem(1, 20m) } };

        // When
        var updateResponse = await client.PutAsJsonAsync($"/api/Sales/{created!.Data!.Id}", updateRequest);

        // Then
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var getResponse = await client.GetAsync($"/api/Sales/{created.Data.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDetailResponse>>(JsonOptions.CaseInsensitive);
        Assert.Single(detail!.Data!.Items);
        Assert.Equal(20m, detail.Data.TotalAmount);
    }

    [Fact(DisplayName = "Given an active sale When cancelling twice Then the second attempt returns 409 Conflict")]
    public async Task Given_ActiveSale_When_CancellingTwice_Then_SecondAttemptReturns409()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var createResponse = await client.PostAsJsonAsync("/api/Sales", ValidSaleRequest(ValidItem(1, 10m)));
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>(JsonOptions.CaseInsensitive);

        var firstCancel = await client.PostAsync($"/api/Sales/{created!.Data!.Id}/cancel", content: null);
        Assert.Equal(HttpStatusCode.OK, firstCancel.StatusCode);

        // When
        var secondCancel = await client.PostAsync($"/api/Sales/{created.Data.Id}/cancel", content: null);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, secondCancel.StatusCode);
        var body = await secondCancel.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions.CaseInsensitive);
        Assert.Equal("Conflict", body?.Type);
    }

    [Fact(DisplayName = "Given two items on a sale When cancelling one Then the total reflects only what remains")]
    public async Task Given_TwoItems_When_CancellingOne_Then_TotalReflectsRemaining()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());
        var createResponse = await client.PostAsJsonAsync("/api/Sales", ValidSaleRequest(ValidItem(1, 20m), ValidItem(1, 30m)));
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<CreateSaleResponse>>(JsonOptions.CaseInsensitive);

        var getResponse = await client.GetAsync($"/api/Sales/{created!.Data!.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDetailResponse>>(JsonOptions.CaseInsensitive);
        var itemToCancel = detail!.Data!.Items.Single(i => i.UnitPrice == 30m);

        // When
        var cancelItemResponse = await client.DeleteAsync($"/api/Sales/{created.Data.Id}/items/{itemToCancel.Id}");

        // Then
        Assert.Equal(HttpStatusCode.OK, cancelItemResponse.StatusCode);
        var reloaded = await (await client.GetAsync($"/api/Sales/{created.Data.Id}"))
            .Content.ReadFromJsonAsync<ApiResponseWithData<SaleDetailResponse>>(JsonOptions.CaseInsensitive);
        Assert.Equal(20m, reloaded!.Data!.TotalAmount);
    }

    [Fact(DisplayName = "Given a non-existent sale id When retrieving it Then returns 404 ResourceNotFound")]
    public async Task Given_NonExistentSale_When_Retrieving_Then_Returns404()
    {
        // Given
        using var client = await AuthHelper.CreateAuthenticatedClientAsync(_fixture.CreateClient());

        // When
        var response = await client.GetAsync($"/api/Sales/{Guid.NewGuid()}");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions.CaseInsensitive);
        Assert.Equal("ResourceNotFound", body?.Type);
    }
}
