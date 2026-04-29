using System.ComponentModel.DataAnnotations;

namespace CardManagement.Api.Models.DTOs.Merchants;

public sealed class CreateMerchantRequest
{
    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Location { get; set; }

    [MaxLength(128)]
    public string? TerminalId { get; set; }

    public string? CustomData { get; set; }
}
