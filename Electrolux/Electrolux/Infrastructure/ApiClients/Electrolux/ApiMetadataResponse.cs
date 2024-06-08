namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public record ApiMetadataResponse
{
    public string Domain { get; init; } = null!;
    public string ApiKey { get; init; } = null!;
    public string Brand { get; init; } = null!;
    public string HttpRegionlBaseUrl { get; init; } = null!;
    public string WebSocketRegionalBaseUrl { get; init; } = null!;
    public string DataCenter { get; init; } = null!;
}