namespace CardManagement.Api.Models.Entities;

public class AutoTopupRule : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string WalletId { get; set; }
    public decimal Threshold { get; set; }
    public decimal TopupAmount { get; set; }
    public int MaxDailyTopups { get; set; } = 3;
    public bool IsActive { get; set; } = true;
    public DateTime? LastTriggered { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Wallet Wallet { get; set; } = null!;
}
