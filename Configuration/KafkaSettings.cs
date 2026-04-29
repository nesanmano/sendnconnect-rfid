namespace CardManagement.Api.Configuration;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = string.Empty;
    public string CardEventsTopic { get; set; } = "card-events";
    public string WalletEventsTopic { get; set; } = "wallet-events";
    public string ClientId { get; set; } = "sendnconnect-rfid-api";
    public string SecurityProtocol { get; set; } = "Plaintext";
    public string SaslMechanism { get; set; } = string.Empty;
    public string SaslUsername { get; set; } = string.Empty;
    public string SaslPassword { get; set; } = string.Empty;
}
