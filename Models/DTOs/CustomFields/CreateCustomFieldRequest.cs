using System.ComponentModel.DataAnnotations;
using CardManagement.Api.Models.Enums;

namespace CardManagement.Api.Models.DTOs.CustomFields;

public sealed class CreateCustomFieldRequest
{
    [Required]
    public CustomFieldEntityType EntityType { get; set; }

    [Required, MaxLength(128)]
    public string FieldKey { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? DisplayName { get; set; }

    [Required]
    public CustomFieldType FieldType { get; set; }

    public bool IsRequired { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsPii { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>JSON array of allowed values for Select/MultiSelect fields.</summary>
    public string? OptionsJson { get; set; }

    [MaxLength(512)]
    public string? DefaultValue { get; set; }

    [MaxLength(512)]
    public string? ValidationRegex { get; set; }
}
