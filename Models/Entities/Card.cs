using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.Entities;

public class Card : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string? HolderId { get; set; }
    public string? CardTypeId { get; set; }
    public string Uid { get; set; } = string.Empty;
    public string? Label { get; set; }
    public CardStatus Status { get; set; } = CardStatus.Active;
    public string? BlockReason { get; set; }
    public string? ReplacedById { get; set; }
    public string? CustomData { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CardHolder? Holder { get; set; }
    public CardType? CardType { get; set; }
    public Card? ReplacedBy { get; set; }
    public Wallet? Wallet { get; set; }
}
