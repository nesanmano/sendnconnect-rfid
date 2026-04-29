using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.DTOs.Cards;
using CardManagement.Api.Models.DTOs.Common;
using CardManagement.Api.Models.Entities;
using CardManagement.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class CardService : ICardService
{
    private readonly CardManagementDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IKafkaProducer _kafka;
    private readonly ILogger<CardService> _logger;

    public CardService(CardManagementDbContext db, ITenantContext tenant, IKafkaProducer kafka, ILogger<CardService> logger)
    {
        _db = db;
        _tenant = tenant;
        _kafka = kafka;
        _logger = logger;
    }

    public async Task<CardResponse> RegisterAsync(RegisterCardRequest request, CancellationToken ct = default)
    {
        // Check blacklist
        var isBlacklisted = await _db.BlacklistedUids
            .AnyAsync(b => b.Uid == request.Uid, ct);
        if (isBlacklisted)
            throw new InvalidOperationException($"UID '{request.Uid}' is blacklisted and cannot be registered.");

        // Check duplicate UID within org
        var exists = await _db.Cards.AnyAsync(c => c.Uid == request.Uid, ct);
        if (exists)
            throw new InvalidOperationException($"A card with UID '{request.Uid}' already exists in this organization.");

        // Resolve card type for expiry
        DateTime? expiresAt = null;
        if (!string.IsNullOrEmpty(request.CardTypeId))
        {
            var cardType = await _db.CardTypes.FindAsync([request.CardTypeId], ct);
            if (cardType is { ValidityDays: > 0 })
                expiresAt = DateTime.UtcNow.AddDays(cardType.ValidityDays.Value);
        }

        var card = new Card
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            Uid = request.Uid,
            Label = request.Label,
            HolderId = request.HolderId,
            CardTypeId = request.CardTypeId,
            Status = CardStatus.Active,
            CustomData = request.CustomData,
            ExpiresAt = expiresAt,
            RegisteredAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Cards.Add(card);

        // Auto-create wallet
        var wallet = new Wallet
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            CardId = card.Id,
            Balance = 0,
            Currency = "USD",
            DailySpent = 0,
            MonthlySpent = 0,
            DailyResetAt = DateTime.UtcNow.Date.AddDays(1),
            MonthlyResetAt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1),
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Try to resolve currency from tenant settings
        var settings = await _db.TenantSettings.FirstOrDefaultAsync(ct);
        if (settings is not null && !string.IsNullOrEmpty(settings.DefaultCurrency))
            wallet.Currency = settings.DefaultCurrency;

        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Card {CardId} registered with UID {Uid}", card.Id, card.Uid);
        await _kafka.PublishCardEventAsync("card.registered", new { card.Id, card.Uid, card.OrgId }, ct);

        return await GetByIdAsync(card.Id, ct);
    }

    public async Task<CardResponse> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var card = await _db.Cards
            .Include(c => c.Holder)
            .Include(c => c.CardType)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Card '{id}' not found.");

        return MapToResponse(card);
    }

    public async Task<PagedResponse<CardResponse>> ListAsync(int page, int pageSize, string? holderId, string? status, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Cards
            .Include(c => c.Holder)
            .Include(c => c.CardType)
            .AsQueryable();

        if (!string.IsNullOrEmpty(holderId))
            query = query.Where(c => c.HolderId == holderId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CardStatus>(status, true, out var s))
            query = query.Where(c => c.Status == s);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.RegisteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<CardResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CardResponse> UpdateAsync(string id, UpdateCardRequest request, CancellationToken ct = default)
    {
        var card = await _db.Cards.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Card '{id}' not found.");

        if (request.Label is not null) card.Label = request.Label;
        if (request.HolderId is not null) card.HolderId = request.HolderId;
        if (request.CardTypeId is not null) card.CardTypeId = request.CardTypeId;
        if (request.CustomData is not null) card.CustomData = request.CustomData;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<CardResponse> BlockAsync(string id, BlockCardRequest request, CancellationToken ct = default)
    {
        var card = await _db.Cards.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Card '{id}' not found.");

        if (card.Status == CardStatus.Wiped)
            throw new InvalidOperationException("Cannot block a wiped card.");

        card.Status = CardStatus.Blocked;
        card.BlockReason = request.Reason;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Card {CardId} blocked: {Reason}", id, request.Reason);
        await _kafka.PublishCardEventAsync("card.blocked", new { card.Id, card.Uid, Reason = request.Reason }, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<CardResponse> UnblockAsync(string id, CancellationToken ct = default)
    {
        var card = await _db.Cards.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Card '{id}' not found.");

        if (card.Status != CardStatus.Blocked)
            throw new InvalidOperationException("Only blocked cards can be unblocked.");

        card.Status = CardStatus.Active;
        card.BlockReason = null;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Card {CardId} unblocked", id);
        await _kafka.PublishCardEventAsync("card.unblocked", new { card.Id, card.Uid }, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<CardResponse> WipeAsync(string id, CancellationToken ct = default)
    {
        var card = await _db.Cards
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Card '{id}' not found.");

        if (card.Status == CardStatus.Wiped)
            throw new InvalidOperationException("Card is already wiped.");

        card.Status = CardStatus.Wiped;
        card.BlockReason = "Card wiped";

        // Zero out wallet balance
        if (card.Wallet is not null)
        {
            card.Wallet.Balance = 0;
            card.Wallet.DailySpent = 0;
            card.Wallet.MonthlySpent = 0;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Card {CardId} wiped", id);
        await _kafka.PublishCardEventAsync("card.wiped", new { card.Id, card.Uid }, ct);

        return await GetByIdAsync(id, ct);
    }

    private static CardResponse MapToResponse(Card c) => new()
    {
        Id = c.Id,
        Uid = c.Uid,
        Label = c.Label,
        Status = c.Status.ToString(),
        BlockReason = c.BlockReason,
        HolderId = c.HolderId,
        HolderName = c.Holder?.DisplayName,
        CardTypeId = c.CardTypeId,
        CardTypeName = c.CardType?.Name,
        ReplacedById = c.ReplacedById,
        CustomData = c.CustomData,
        ExpiresAt = c.ExpiresAt,
        RegisteredAt = c.RegisteredAt,
        UpdatedAt = c.UpdatedAt
    };
}
