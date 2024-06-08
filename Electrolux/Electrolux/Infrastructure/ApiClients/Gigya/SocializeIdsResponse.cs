namespace Electrolux.Infrastructure.ApiClients.Gigya;

[Serializable]
public record SocializeIdsResponse
{
    public string Gmid { get; init; } = null!;
    public string Gcid { get; init; } = null!;
    public string Ucid { get; init; } = null!;
}