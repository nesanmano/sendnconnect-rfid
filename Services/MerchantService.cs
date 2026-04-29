using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.DTOs.Common;
using CardManagement.Api.Models.DTOs.Merchants;
using CardManagement.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class MerchantService : IMerchantService
{
    private readonly CardManagementDbContext _db;
    private readonly ITenantContext _tenant;

    public MerchantService(CardManagementDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<MerchantResponse> CreateAsync(CreateMerchantRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(request.TerminalId))
        {
            var exists = await _db.Merchants.AnyAsync(m => m.TerminalId == request.TerminalId, ct);
            if (exists)
                throw new InvalidOperationException($"A merchant with terminal ID '{request.TerminalId}' already exists.");
        }

        var merchant = new Merchant
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            Name = request.Name,
            Location = request.Location,
            TerminalId = request.TerminalId,
            IsActive = true,
            CustomData = request.CustomData,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Merchants.Add(merchant);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(merchant);
    }

    public async Task<MerchantResponse> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Merchant '{id}' not found.");
        return MapToResponse(merchant);
    }

    public async Task<PagedResponse<MerchantResponse>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Merchants.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(s) ||
                (m.TerminalId != null && m.TerminalId.ToLower().Contains(s)) ||
                (m.Location != null && m.Location.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<MerchantResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MerchantResponse> UpdateAsync(string id, UpdateMerchantRequest request, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Merchant '{id}' not found.");

        if (request.Name is not null) merchant.Name = request.Name;
        if (request.Location is not null) merchant.Location = request.Location;
        if (request.TerminalId is not null) merchant.TerminalId = request.TerminalId;
        if (request.IsActive.HasValue) merchant.IsActive = request.IsActive.Value;
        if (request.CustomData is not null) merchant.CustomData = request.CustomData;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(merchant);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Merchant '{id}' not found.");

        merchant.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    private static MerchantResponse MapToResponse(Merchant m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Location = m.Location,
        TerminalId = m.TerminalId,
        IsActive = m.IsActive,
        CustomData = m.CustomData,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
