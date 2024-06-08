namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public record ApplianceCapabilitiesResponse
{
    public string Access { get; init; } = null!;
    public string Type { get; init; } = null!;
    public int? Step { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    public bool? Schedulable { get; init; }
    public Dictionary<string, object>? Values { get; init; }
}