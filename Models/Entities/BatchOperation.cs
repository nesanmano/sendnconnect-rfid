using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.Entities;

public class BatchOperation : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public BatchOperationType Type { get; set; }
    public BatchOperationStatus Status { get; set; } = BatchOperationStatus.Pending;
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string? ErrorDetails { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
