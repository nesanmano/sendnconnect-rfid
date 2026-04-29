namespace CardManagement.Api.Models.Entities;

public class CardType : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public decimal? DailySpendLimit { get; set; }
    public decimal? MonthlySpendLimit { get; set; }
    public decimal? MaxBalance { get; set; }
    public int? ValidityDays { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CustomData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
