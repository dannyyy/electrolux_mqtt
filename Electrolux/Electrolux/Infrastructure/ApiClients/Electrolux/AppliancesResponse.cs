namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public record AppliancesResponse
{
    public string ApplianceId { get; init; } = null!;
    public ApplianceDataResponse ApplianceData { get; init; } = null!;

    [Serializable]
    public record ApplianceDataResponse
    {
        public string ApplianceName { get; init; } = null!;
        public string ModelName { get; init; } = null!;
    }
}