using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.Cards;

public sealed class RegisterCardRequest
{
    [Required, MaxLength(128)]
    public string Uid { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Label { get; set; }

    /// <summary>Card holder ID (optional — assign later).</summary>
    public string? HolderId { get; set; }

    /// <summary>Card type ID (optional — uses org defaults if omitted).</summary>
    public string? CardTypeId { get; set; }

    /// <summary>Arbitrary tenant-specific data stored as JSON.</summary>
    public string? CustomData { get; set; }
}
