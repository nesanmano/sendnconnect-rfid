using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.DTOs.Transactions;

public sealed class TransactionQueryParams
{
    public string? WalletId { get; set; }
    public string? CardId { get; set; }
    public TransactionType? Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? MerchantId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
