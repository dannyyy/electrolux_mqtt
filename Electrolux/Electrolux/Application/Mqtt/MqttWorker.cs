using Electrolux.Domain.UseCases.GetAppliances;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Electrolux.Application.Mqtt;

public class MqttWorker : BackgroundService
{
    private readonly ILogger<MqttWorker> _logger;
    private readonly IMqttControlService _mqttControlService;
    private readonly ApplianceService _applianceService;
    private readonly ApplicationOptions _applicationOptions;

    public MqttWorker(
        ILogger<MqttWorker> logger,
        IOptions<ApplicationOptions> applicationOptions,
        IMqttControlService mqttControlService,
        ApplianceService applianceService)
    {
        _logger = logger;
        _mqttControlService = mqttControlService;
        _applianceService = applianceService;
        _applicationOptions = applicationOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connect to MQTT server");
        await _mqttControlService.ConnectAsync(cancellationToken);
        await _applianceService.RegisterCommandsHandlerAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            await _applianceService.GetAppliancesAndPublishCapabilitiesAndStateAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(_applicationOptions.StatusUpdateInterval), cancellationToken);
        }

        _logger.LogInformation("Disconnect from MQTT server");
        await _mqttControlService.DisconnectAsync(cancellationToken);
    }
}