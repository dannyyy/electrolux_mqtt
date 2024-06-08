namespace Electrolux.Infrastructure.Mqtt;

[Serializable]
public record MqttOptions
{
    public string Host { get; init; } = null!;
    public int Port { get; init; }
    public bool UseTls { get; init; }
    public string TlsSha256Fingerprint { get; init; } = null!;
    public bool TrustAllCertificates { get; init; }
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string TopicPrefix { get; init; } = null!;
}