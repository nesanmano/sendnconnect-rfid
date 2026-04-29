namespace CardManagement.Api.Models.DTOs.Transactions;

public sealed class TransactionResponse
{
    public string Id { get; set; } = string.Empty;
    public string WalletId { get; set; } = string.Empty;
    public string? MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Reference { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? RelatedTxnId { get; set; }
    public string? Description { get; set; }
    public string? CustomData { get; set; }
    public DateTime CreatedAt { get; set; }
}
