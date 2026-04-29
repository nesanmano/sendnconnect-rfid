using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.Wallets;

public sealed class RefundWalletRequest
{
    /// <summary>Original transaction to refund.</summary>
    [Required]
    public string OriginalTransactionId { get; set; } = string.Empty;

    [Required, Range(0.0001, 999999999)]
    public decimal Amount { get; set; }

    [MaxLength(256)]
    public string? Reference { get; set; }

    [Required, MaxLength(128)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Description { get; set; }

    public string? CustomData { get; set; }
}
