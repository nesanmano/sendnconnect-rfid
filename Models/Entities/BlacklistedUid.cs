namespace CardManagement.Api.Models.Entities;

public class BlacklistedUid : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string BlockedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
