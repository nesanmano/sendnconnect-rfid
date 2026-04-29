using CardManagement.Api.Models.DTOs.CardHolders;
using CardManagement.Api.Models.DTOs.Common;

namespace CardManagement.Api.Services;

public interface ICardHolderService
{
    Task<CardHolderResponse> CreateAsync(CreateCardHolderRequest request, CancellationToken ct = default);
    Task<CardHolderResponse> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PagedResponse<CardHolderResponse>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<CardHolderResponse> UpdateAsync(string id, UpdateCardHolderRequest request, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
