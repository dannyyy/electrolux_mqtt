namespace Electrolux.Domain.Shared;

public interface IMqttSubscriptionService
{
    Task SubscribeAsync(string topicSuffix, Func<string, string, Task> action, CancellationToken cancellationToken);
}