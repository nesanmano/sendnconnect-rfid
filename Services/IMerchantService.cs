using CardManagement.Api.Models.DTOs.Common;
using CardManagement.Api.Models.DTOs.Merchants;

namespace CardManagement.Api.Services;

public interface IMerchantService
{
    Task<MerchantResponse> CreateAsync(CreateMerchantRequest request, CancellationToken ct = default);
    Task<MerchantResponse> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PagedResponse<MerchantResponse>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<MerchantResponse> UpdateAsync(string id, UpdateMerchantRequest request, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
