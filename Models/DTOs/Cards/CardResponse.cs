namespace CardManagement.Api.Models.DTOs.Cards;

public sealed class CardResponse
{
    public string Id { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? BlockReason { get; set; }
    public string? HolderId { get; set; }
    public string? HolderName { get; set; }
    public string? CardTypeId { get; set; }
    public string? CardTypeName { get; set; }
    public string? ReplacedById { get; set; }
    public string? CustomData { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
