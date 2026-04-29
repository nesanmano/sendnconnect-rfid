using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.TenantSettings;

public sealed class UpdateTenantSettingsRequest
{
    [MaxLength(256)]
    public string? DisplayName { get; set; }

    [MaxLength(8)]
    public string? DefaultCurrency { get; set; }

    [MaxLength(64)]
    public string? Timezone { get; set; }

    public int? MaxCardsPerHolder { get; set; }
    public bool? AllowNegativeBalance { get; set; }

    /// <summary>Free-form JSON for any extra tenant config.</summary>
    public string? SettingsJson { get; set; }
}
