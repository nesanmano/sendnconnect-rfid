namespace CardManagement.Api.Models.DTOs.TenantSettings;

public sealed class TenantSettingsResponse
{
    public string Id { get; set; } = string.Empty;
    public string OrgId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public string Timezone { get; set; } = "UTC";
    public int MaxCardsPerHolder { get; set; }
    public bool AllowNegativeBalance { get; set; }
    public string? SettingsJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
