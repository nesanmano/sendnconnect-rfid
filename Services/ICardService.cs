using CardManagement.Api.Models.DTOs.Cards;
using CardManagement.Api.Models.DTOs.Common;

namespace CardManagement.Api.Services;

public interface ICardService
{
    Task<CardResponse> RegisterAsync(RegisterCardRequest request, CancellationToken ct = default);
    Task<CardResponse> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PagedResponse<CardResponse>> ListAsync(int page, int pageSize, string? holderId, string? status, CancellationToken ct = default);
    Task<CardResponse> UpdateAsync(string id, UpdateCardRequest request, CancellationToken ct = default);
    Task<CardResponse> BlockAsync(string id, BlockCardRequest request, CancellationToken ct = default);
    Task<CardResponse> UnblockAsync(string id, CancellationToken ct = default);
    Task<CardResponse> WipeAsync(string id, CancellationToken ct = default);
}
