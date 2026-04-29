using System.Text.Json;
using CardManagement.Api.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CardManagement.Api.Infrastructure;

public interface IKafkaProducer
{
    Task PublishCardEventAsync(string eventType, object data, CancellationToken ct = default);
    Task PublishWalletEventAsync(string eventType, object data, CancellationToken ct = default);
}

public sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaSettings> settings, IServiceScopeFactory scopeFactory, ILogger<KafkaProducer> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            ClientId = _settings.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3
        };

        if (!string.IsNullOrEmpty(_settings.SecurityProtocol) &&
            Enum.TryParse<SecurityProtocol>(_settings.SecurityProtocol, true, out var protocol))
            config.SecurityProtocol = protocol;

        if (!string.IsNullOrEmpty(_settings.SaslMechanism))
        {
            if (Enum.TryParse<SaslMechanism>(_settings.SaslMechanism, true, out var mechanism))
                config.SaslMechanism = mechanism;
            config.SaslUsername = _settings.SaslUsername;
            config.SaslPassword = _settings.SaslPassword;
        }

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task PublishCardEventAsync(string eventType, object data, CancellationToken ct = default)
        => PublishAsync(_settings.CardEventsTopic, eventType, data, ct);

    public Task PublishWalletEventAsync(string eventType, object data, CancellationToken ct = default)
        => PublishAsync(_settings.WalletEventsTopic, eventType, data, ct);

    private async Task PublishAsync(string topic, string eventType, object data, CancellationToken ct)
    {
        string? orgId = null;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tenantCtx = scope.ServiceProvider.GetService<ITenantContext>();
            orgId = tenantCtx?.OrgId;
        }
        catch { /* best-effort tenant resolution */ }

        var cloudEvent = new
        {
            specversion = "1.0",
            type = eventType,
            source = "/card-management-api",
            id = Guid.NewGuid().ToString(),
            time = DateTime.UtcNow.ToString("O"),
            datacontenttype = "application/json",
            subject = orgId,
            data
        };

        var json = JsonSerializer.Serialize(cloudEvent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var key = orgId ?? "unknown";

        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = json }, ct);
            _logger.LogDebug("Published {EventType} to {Topic} partition {Partition} offset {Offset}",
                eventType, topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType} to {Topic}", eventType, topic);
        }
    }

    public void Dispose() => _producer?.Dispose();
}
