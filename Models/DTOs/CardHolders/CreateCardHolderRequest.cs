using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.CardHolders;

public sealed class CreateCardHolderRequest
{
    [MaxLength(256)]
    public string? ExternalId { get; set; }

    [MaxLength(256)]
    public string? DisplayName { get; set; }

    [MaxLength(256), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    /// <summary>Arbitrary JSON — tenant stores whatever they want.</summary>
    public string? CustomData { get; set; }
}
