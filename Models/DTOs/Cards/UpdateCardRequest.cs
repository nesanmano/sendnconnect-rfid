using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.Cards;

public sealed class UpdateCardRequest
{
    [MaxLength(256)]
    public string? Label { get; set; }

    public string? HolderId { get; set; }
    public string? CardTypeId { get; set; }
    public string? CustomData { get; set; }
}
