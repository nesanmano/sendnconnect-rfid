namespace CardManagement.Api.Models.DTOs.Wallets;

public sealed class WalletResponse
{
    public string Id { get; set; } = string.Empty;
    public string CardId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal DailySpent { get; set; }
    public decimal MonthlySpent { get; set; }
    public DateTime? DailyResetAt { get; set; }
    public DateTime? MonthlyResetAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
