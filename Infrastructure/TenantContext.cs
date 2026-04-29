namespace CardManagement.Api.Infrastructure;

public sealed class TenantContext : ITenantContext
{
    public string OrgId { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public bool IsResolved { get; private set; }

    public void SetTenant(string orgId, string userId)
    {
        OrgId = orgId ?? throw new ArgumentNullException(nameof(orgId));
        UserId = userId ?? string.Empty;
        IsResolved = true;
    }
}
