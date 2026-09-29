using Microsoft.AspNetCore.Mvc;

namespace SalesApi.WebApi.Features.Users.ListUsers;

/// <summary>
/// Request model for listing users, following the API's convention:
/// _page/_size for pagination, _order for sorting - same as ListSalesRequest.
/// </summary>
public class ListUsersRequest
{
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "_order")]
    public string? Order { get; set; }
}