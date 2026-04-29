namespace CardManagement.Api.Models.DTOs.Merchants;

public sealed class MerchantResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? TerminalId { get; set; }
    public bool IsActive { get; set; }
    public string? CustomData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
