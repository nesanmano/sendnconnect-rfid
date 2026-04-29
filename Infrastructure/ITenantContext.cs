namespace CardManagement.Api.Infrastructure;

public interface ITenantContext
{
    string OrgId { get; }
    string UserId { get; }
    bool IsResolved { get; }
    void SetTenant(string orgId, string userId);
}
