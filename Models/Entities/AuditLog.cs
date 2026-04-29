namespace CardManagement.Api.Models.Entities;

public class AuditLog : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ChangesJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
