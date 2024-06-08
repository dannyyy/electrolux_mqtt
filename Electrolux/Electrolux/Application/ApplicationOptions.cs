namespace Electrolux.Application;

[Serializable]
public record ApplicationOptions
{
    public int StatusUpdateInterval { get; init; }
    public bool UseMqtt { get; init; }
    public bool UseHttp { get; init; }
}