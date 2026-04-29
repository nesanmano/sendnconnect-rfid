using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.Cards;

public sealed class BlockCardRequest
{
    [Required, MaxLength(512)]
    public string Reason { get; set; } = string.Empty;
}
