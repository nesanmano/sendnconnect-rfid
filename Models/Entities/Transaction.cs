using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.Entities;

public class Transaction : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string WalletId { get; set; }
    public string? MerchantId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Reference { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? RelatedTxnId { get; set; }
    public string? Description { get; set; }
    public string? CustomData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet Wallet { get; set; } = null!;
    public Merchant? Merchant { get; set; }
    public Transaction? RelatedTxn { get; set; }
}
