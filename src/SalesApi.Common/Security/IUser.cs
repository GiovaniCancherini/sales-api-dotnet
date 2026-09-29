namespace SalesApi.Common.Security
{
    /// <summary>
    /// Contract for representing the current user (e.g. from claims).
    /// </summary>
    public interface IUser
    {
        public string Id { get; }
        public string Username { get; }
        public string Role { get; }
    }
}
