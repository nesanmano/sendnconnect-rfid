using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.Wallets;

public sealed class LoadWalletRequest
{
    [Required, Range(0.0001, 999999999)]
    public decimal Amount { get; set; }

    [MaxLength(256)]
    public string? Reference { get; set; }

    [MaxLength(128)]
    public string? IdempotencyKey { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    public string? CustomData { get; set; }
}
