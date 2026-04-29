namespace CardManagement.Api.Configuration;

public sealed class ZitadelSettings
{
    public const string SectionName = "Zitadel";
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}
