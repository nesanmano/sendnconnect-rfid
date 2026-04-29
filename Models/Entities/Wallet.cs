namespace CardManagement.Api.Models.Entities;

public class Wallet : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string CardId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal DailySpent { get; set; }
    public DateTime? DailyResetAt { get; set; }
    public decimal MonthlySpent { get; set; }
    public DateTime? MonthlyResetAt { get; set; }
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Card Card { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public AutoTopupRule? AutoTopupRule { get; set; }
}
