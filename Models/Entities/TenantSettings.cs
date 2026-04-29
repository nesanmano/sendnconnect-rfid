namespace CardManagement.Api.Models.Entities;

public class TenantSettings : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public string Timezone { get; set; } = "UTC";
    public int MaxCardsPerHolder { get; set; } = 3;
    public bool AllowNegativeBalance { get; set; }
    public string? SettingsJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
