using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Electrolux.Domain.Shared;
using Electrolux.Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Electrolux.Domain.UseCases.GetAppliances;

public class ApplianceService
{
    private readonly ILogger<ApplianceService> _logger;
    private readonly ElectroluxRepository _electroluxRepository;
    private readonly IMqttPublishingService _mqttPublishingService;
    private readonly IMqttSubscriptionService _mqttSubscriptionService;

    public ApplianceService(
        ILogger<ApplianceService> logger,
        ElectroluxRepository electroluxRepository,
        IMqttPublishingService mqttPublishingService,
        IMqttSubscriptionService mqttSubscriptionService)
    {
        _logger = logger;
        _electroluxRepository = electroluxRepository;
        _mqttPublishingService = mqttPublishingService;
        _mqttSubscriptionService = mqttSubscriptionService;
    }

    public async Task RegisterCommandsHandlerAsync(CancellationToken cancellationToken)
    {
        await _mqttSubscriptionService.SubscribeAsync(
            "appliances/+/command",
            async (t, p) =>
            {
                var applianceId = Regex.Match(t, "appliances/([0-9]+)/command").Groups[1].Value;
                var commands = JsonSerializer.Deserialize<Dictionary<string, object>>(p);
                if (commands == null)
                    return;

                foreach (var command in commands)
                {
                    var applianceCommand = new ApplianceCommand(command.Key, command.Value);
                    await SendApplianceCommandAsync(applianceId, applianceCommand, cancellationToken);
                    await PublishStateAsync(applianceId, cancellationToken);
                    _logger.LogInformation(
                        "Sent {Command}={Value} to appliance {ApplianceId}",
                        applianceCommand.Command,
                        applianceCommand.Value,
                        applianceId);
                }
            },
            cancellationToken);
    }

    public async Task GetAppliancesAndPublishCapabilitiesAndStateAsync(CancellationToken cancellationToken)
    {
        var appliances = await _electroluxRepository.GetAppliances(cancellationToken);
        await _mqttPublishingService.PublishAsync(
            topicSuffix: "appliances",
            payload: JsonSerializer.Serialize(appliances),
            retain: false,
            cancellationToken: cancellationToken);

        foreach (var appliance in appliances)
        {
            await PublishCapabilitiesAsync(appliance.ApplianeId, cancellationToken);
            await PublishStateAsync(appliance.ApplianeId, cancellationToken);
        }
    }

    private async Task PublishStateAsync(string applianceId, CancellationToken cancellationToken)
    {
        var states = await _electroluxRepository.GetStateByApplianceAsync(applianceId, cancellationToken);
        await _mqttPublishingService.PublishAsync(
            topicSuffix: $"appliances/{applianceId}/state",
            payload: JsonSerializer.Serialize(states.States),
            retain: false,
            cancellationToken: cancellationToken);
    }

    private async Task PublishCapabilitiesAsync(string applianceId, CancellationToken cancellationToken)
    {
        var capabilities = (await _electroluxRepository.GetCapabilitiesByApplianceAsync(applianceId, cancellationToken))
            .ToDictionary(k => k.command, v => new {Values = v.values, Min = v.min, Max = v.max});
        await _mqttPublishingService.PublishAsync(
            topicSuffix: $"appliances/{applianceId}/capabilities",
            payload: JsonSerializer.Serialize(capabilities, new JsonSerializerOptions {DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull}),
            retain: false,
            cancellationToken: cancellationToken);
    }

    private async Task SendApplianceCommandAsync(string applianceId, ApplianceCommand applianceCommand, CancellationToken cancellationToken)
    {
        await _electroluxRepository.SendCommandAsync(applianceId, applianceCommand, cancellationToken);
    }
}