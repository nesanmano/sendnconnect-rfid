using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.Entities;

public class CustomFieldDefinition : ITenantEntity
{
    public string Id { get; set; }
    public string OrgId { get; set; } = string.Empty;
    public CustomFieldEntityType EntityType { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;
    public bool IsRequired { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsPii { get; set; }
    public int DisplayOrder { get; set; }
    public string? OptionsJson { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRegex { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
