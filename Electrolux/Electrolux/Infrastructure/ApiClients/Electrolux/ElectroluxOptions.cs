namespace Electrolux.Infrastructure.ApiClients.Electrolux;

[Serializable]
public record ElectroluxOptions
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
}