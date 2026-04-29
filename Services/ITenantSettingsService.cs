using CardManagement.Api.Models.DTOs.TenantSettings;

namespace CardManagement.Api.Services;

public interface ITenantSettingsService
{
    Task<TenantSettingsResponse> GetAsync(CancellationToken ct = default);
    Task<TenantSettingsResponse> UpdateAsync(UpdateTenantSettingsRequest request, CancellationToken ct = default);
}
