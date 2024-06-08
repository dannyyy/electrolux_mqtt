using System.Security.Cryptography;
using Electrolux.Application.Mqtt;
using Electrolux.Domain.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace Electrolux.Infrastructure.Mqtt;

public class MqttService : IMqttService, IMqttControlService, IMqttPublishingService, IMqttSubscriptionService
{
    private readonly ILogger<MqttService> _logger;
    private readonly MqttOptions _mqttOptions;
    private IMqttClient? _client;

    public MqttService(ILogger<MqttService> logger, IOptions<MqttOptions> mqttOptions)
    {
        _logger = logger;
        _mqttOptions = mqttOptions.Value;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Trying to connect to MQTT server {MqttHost} on port {MqttPort}",
            _mqttOptions.Host,
            _mqttOptions.Port);

        var mqttFactory = new MqttFactory();

        _client = mqttFactory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttOptions.Host, _mqttOptions.Port)
            .WithCredentials(_mqttOptions.Username, _mqttOptions.Password)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCleanSession()
            .WithTls(o =>
            {
                o.UseTls = _mqttOptions.UseTls;
                if (_mqttOptions.UseTls && _mqttOptions.TrustAllCertificates)
                {
                    o.AllowUntrustedCertificates = true;
                    o.IgnoreCertificateChainErrors = true;
                    o.IgnoreCertificateRevocationErrors = true;
                }
                else if (_mqttOptions.UseTls && !string.IsNullOrWhiteSpace(_mqttOptions.TlsSha256Fingerprint))
                {
                    o.CertificateValidationHandler = c => c.Certificate.GetCertHashString(HashAlgorithmName.SHA256).Equals(_mqttOptions.TlsSha256Fingerprint, StringComparison.OrdinalIgnoreCase);
                }
            })
            .Build();

        using var timeout = new CancellationTokenSource(5000);
        await _client.ConnectAsync(options, timeout.Token);

        _logger.LogInformation("Connected to MQTT server {MqttHost} on port {MqttPort}",
            _mqttOptions.Host,
            _mqttOptions.Port);

    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (_client?.IsConnected ?? false)
        {
            await _client.DisconnectAsync(cancellationToken: cancellationToken);
            _client.Dispose();
            _client = null;
        }
    }

    public async Task PublishAsync(string topicSuffix, string payload, bool retain, CancellationToken cancellationToken)
    {
        await EnsureConnectionAsync(cancellationToken);

        var topic = GetFullTopicName(topicSuffix);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithContentType("application/json")
            .WithPayload(payload)
            .WithRetainFlag(retain)
            .Build();

        _logger.LogDebug("Publishing message to {Topic}", topic);
        await _client!.PublishAsync(message, cancellationToken);
        _logger.LogDebug("Message published");
    }

    public async Task SubscribeAsync(string topicSuffix, Func<string, string, Task> action, CancellationToken cancellationToken)
    {
        await EnsureConnectionAsync(cancellationToken);

        var topic = GetFullTopicName(topicSuffix);
        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic).WithNoLocal()
            .Build();
        await _client!.SubscribeAsync(topicFilter, cancellationToken);
        _client!.ApplicationMessageReceivedAsync += args => action(args.ApplicationMessage.Topic, args.ApplicationMessage.ConvertPayloadToString());
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_client?.IsConnected == true)
        {
            return;
        }

        if (_client != null)
        {
            await DisconnectAsync(cancellationToken);
        }

        await ConnectAsync(cancellationToken);
    }

    private string GetFullTopicName(string topicSuffix)
    {
        return $"{_mqttOptions.TopicPrefix}{topicSuffix}";
    }
}