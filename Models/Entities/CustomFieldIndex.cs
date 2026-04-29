using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.Entities;

public class CustomFieldIndex : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public CustomFieldEntityType EntityType { get; set; }
    public string EntityId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
}
