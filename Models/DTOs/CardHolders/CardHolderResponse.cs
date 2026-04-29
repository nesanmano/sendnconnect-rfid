namespace CardManagement.Api.Models.DTOs.CardHolders;

public sealed class CardHolderResponse
{
    public string Id { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public string? CustomData { get; set; }
    public int CardCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
