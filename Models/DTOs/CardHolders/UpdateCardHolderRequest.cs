using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.CardHolders;

public sealed class UpdateCardHolderRequest
{
    [MaxLength(256)]
    public string? DisplayName { get; set; }

    [MaxLength(256), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    public bool? IsActive { get; set; }
    public string? CustomData { get; set; }
}
