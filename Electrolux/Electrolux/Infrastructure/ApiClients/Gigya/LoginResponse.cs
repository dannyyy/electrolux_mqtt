namespace Electrolux.Infrastructure.ApiClients.Gigya;

[Serializable]
public record LoginResponse
{
    public string UID { get; init; } = null!;

    public LoginSessionInfo SessionInfo { get; init; } = null!;

    [Serializable]
    public record LoginSessionInfo
    {
        public string SessionToken { get; init; } = null!;
        public string SessionSecret { get; init; } = null!;
    }
}