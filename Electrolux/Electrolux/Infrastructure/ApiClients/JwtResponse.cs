using System.Text.Json.Serialization;

namespace Electrolux.Infrastructure.ApiClients;

[Serializable]
public record JwtResponse
{
    [JsonPropertyName("id_token")]
    public string IdToken { get; init; } = null!;

    public string AccessToken { get; init; } = null!;

    public string RefreshToken { get; init; } = null!;

    public string Scope { get; init; } = null!;
}