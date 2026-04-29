using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.DTOs.CardHolders;
using CardManagement.Api.Models.DTOs.Common;
using CardManagement.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class CardHolderService : ICardHolderService
{
    private readonly CardManagementDbContext _db;
    private readonly ITenantContext _tenant;

    public CardHolderService(CardManagementDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CardHolderResponse> CreateAsync(CreateCardHolderRequest request, CancellationToken ct = default)
    {
        // Check unique external_id if provided
        if (!string.IsNullOrEmpty(request.ExternalId))
        {
            var exists = await _db.CardHolders.AnyAsync(h => h.ExternalId == request.ExternalId, ct);
            if (exists)
                throw new InvalidOperationException($"A card holder with external ID '{request.ExternalId}' already exists.");
        }

        var holder = new CardHolder
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            ExternalId = request.ExternalId,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Phone = request.Phone,
            IsActive = true,
            CustomData = request.CustomData,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.CardHolders.Add(holder);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(holder, 0);
    }

    public async Task<CardHolderResponse> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var holder = await _db.CardHolders
            .Include(h => h.Cards)
            .FirstOrDefaultAsync(h => h.Id == id, ct)
            ?? throw new KeyNotFoundException($"Card holder '{id}' not found.");

        return MapToResponse(holder, holder.Cards?.Count ?? 0);
    }

    public async Task<PagedResponse<CardHolderResponse>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.CardHolders.Include(h => h.Cards).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(h =>
                (h.DisplayName != null && h.DisplayName.ToLower().Contains(s)) ||
                (h.Email != null && h.Email.ToLower().Contains(s)) ||
                (h.ExternalId != null && h.ExternalId.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<CardHolderResponse>
        {
            Items = items.Select(h => MapToResponse(h, h.Cards?.Count ?? 0)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CardHolderResponse> UpdateAsync(string id, UpdateCardHolderRequest request, CancellationToken ct = default)
    {
        var holder = await _db.CardHolders
            .Include(h => h.Cards)
            .FirstOrDefaultAsync(h => h.Id == id, ct)
            ?? throw new KeyNotFoundException($"Card holder '{id}' not found.");

        if (request.DisplayName is not null) holder.DisplayName = request.DisplayName;
        if (request.Email is not null) holder.Email = request.Email;
        if (request.Phone is not null) holder.Phone = request.Phone;
        if (request.IsActive.HasValue) holder.IsActive = request.IsActive.Value;
        if (request.CustomData is not null) holder.CustomData = request.CustomData;

        await _db.SaveChangesAsync(ct);

        return MapToResponse(holder, holder.Cards?.Count ?? 0);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var holder = await _db.CardHolders.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Card holder '{id}' not found.");

        holder.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    private static CardHolderResponse MapToResponse(CardHolder h, int cardCount) => new()
    {
        Id = h.Id,
        ExternalId = h.ExternalId,
        DisplayName = h.DisplayName,
        Email = h.Email,
        Phone = h.Phone,
        IsActive = h.IsActive,
        CustomData = h.CustomData,
        CardCount = cardCount,
        CreatedAt = h.CreatedAt,
        UpdatedAt = h.UpdatedAt
    };
}
