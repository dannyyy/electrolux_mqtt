namespace Electrolux.Domain.Shared;

public interface IMqttPublishingService
{
    Task PublishAsync(string topicSuffix, string payload, bool retain, CancellationToken cancellationToken);
}