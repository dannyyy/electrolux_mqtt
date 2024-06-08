namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public record ApplianceStateResponse
{
    public ApplianceStatePropertiesResponse Properties { get; init; } = null!;
    public string ConnectionState { get; init; } = null!;

    [Serializable]
    public class ApplianceStatePropertiesResponse
    {
        public Dictionary<string, object> Reported { get; init; } = null!;
    }
}