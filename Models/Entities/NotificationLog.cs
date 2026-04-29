using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.Entities;

public class NotificationLog : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public string? HolderId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? BodyPreview { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Queued;
    public string? FailureReason { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CardHolder? Holder { get; set; }
}
