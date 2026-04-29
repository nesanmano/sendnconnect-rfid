using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.DTOs.TenantSettings;
using CardManagement.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class TenantSettingsService : ITenantSettingsService
{
    private readonly CardManagementDbContext _db;
    private readonly ITenantContext _tenant;

    public TenantSettingsService(CardManagementDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<TenantSettingsResponse> GetAsync(CancellationToken ct = default)
    {
        var settings = await _db.TenantSettings.FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            // Auto-create defaults
            settings = new TenantSettings
            {
                Id = Guid.NewGuid().ToString(),
                OrgId = _tenant.OrgId,
                DefaultCurrency = "USD",
                Timezone = "UTC",
                MaxCardsPerHolder = 5,
                AllowNegativeBalance = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.TenantSettings.Add(settings);
            await _db.SaveChangesAsync(ct);
        }

        return MapToResponse(settings);
    }

    public async Task<TenantSettingsResponse> UpdateAsync(UpdateTenantSettingsRequest request, CancellationToken ct = default)
    {
        var settings = await _db.TenantSettings.FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            settings = new TenantSettings
            {
                Id = Guid.NewGuid().ToString(),
                OrgId = _tenant.OrgId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.TenantSettings.Add(settings);
        }

        if (request.DisplayName is not null) settings.DisplayName = request.DisplayName;
        if (request.DefaultCurrency is not null) settings.DefaultCurrency = request.DefaultCurrency;
        if (request.Timezone is not null) settings.Timezone = request.Timezone;
        if (request.MaxCardsPerHolder.HasValue) settings.MaxCardsPerHolder = request.MaxCardsPerHolder.Value;
        if (request.AllowNegativeBalance.HasValue) settings.AllowNegativeBalance = request.AllowNegativeBalance.Value;
        if (request.SettingsJson is not null) settings.SettingsJson = request.SettingsJson;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(settings);
    }

    private static TenantSettingsResponse MapToResponse(TenantSettings s) => new()
    {
        Id = s.Id,
        OrgId = s.OrgId,
        DisplayName = s.DisplayName,
        DefaultCurrency = s.DefaultCurrency ?? "USD",
        Timezone = s.Timezone ?? "UTC",
        MaxCardsPerHolder = s.MaxCardsPerHolder,
        AllowNegativeBalance = s.AllowNegativeBalance,
        SettingsJson = s.SettingsJson,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
