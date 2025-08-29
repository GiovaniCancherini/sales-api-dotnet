namespace SalesApi.WebApi.Features.Users.ListUsers;

/// <summary>
/// Request model for listing users
/// </summary>
public class ListUsersRequest
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}