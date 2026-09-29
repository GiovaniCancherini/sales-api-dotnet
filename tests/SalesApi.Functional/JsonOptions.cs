using System.Text.Json;

namespace SalesApi.Functional;

/// <summary>
/// The API serializes with camelCase property names; System.Net.Http.Json's
/// ReadFromJsonAsync defaults to case-sensitive matching, which would
/// silently leave every PascalCase-named property at its default value.
/// </summary>
internal static class JsonOptions
{
    public static readonly JsonSerializerOptions CaseInsensitive = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
