namespace Electrolux.Application.Mqtt;

public interface IMqttControlService
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
}