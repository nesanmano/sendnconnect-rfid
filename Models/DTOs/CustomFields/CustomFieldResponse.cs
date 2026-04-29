using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.DTOs.CustomFields;

public sealed class CustomFieldResponse
{
    public string Id { get; set; } = string.Empty;
    public CustomFieldEntityType EntityType { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsPii { get; set; }
    public int DisplayOrder { get; set; }
    public string? OptionsJson { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRegex { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
