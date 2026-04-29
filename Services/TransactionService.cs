using CardManagement.Api.Data;
using CardManagement.Api.Models.DTOs.Common;
using CardManagement.Api.Models.DTOs.Transactions;
using CardManagement.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly CardManagementDbContext _db;

    public TransactionService(CardManagementDbContext db)
    {
        _db = db;
    }

    public async Task<TransactionResponse> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var txn = await _db.Transactions
            .Include(t => t.Merchant)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new KeyNotFoundException($"Transaction '{id}' not found.");

        return MapToResponse(txn);
    }

    public async Task<PagedResponse<TransactionResponse>> ListAsync(TransactionQueryParams query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.Transactions
            .Include(t => t.Merchant)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query.WalletId))
            q = q.Where(t => t.WalletId == query.WalletId);

        if (!string.IsNullOrEmpty(query.CardId))
        {
            var walletIds = await _db.Wallets
                .Where(w => w.CardId == query.CardId)
                .Select(w => w.Id)
                .ToListAsync(ct);
            q = q.Where(t => walletIds.Contains(t.WalletId));
        }

        if (query.Type.HasValue)
            q = q.Where(t => t.Type == query.Type.Value);

        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);

        if (!string.IsNullOrEmpty(query.MerchantId))
            q = q.Where(t => t.MerchantId == query.MerchantId);

        if (query.FromDate.HasValue)
            q = q.Where(t => t.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(t => t.CreatedAt <= query.ToDate.Value);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<TransactionResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static TransactionResponse MapToResponse(Transaction t) => new()
    {
        Id = t.Id,
        WalletId = t.WalletId,
        MerchantId = t.MerchantId,
        MerchantName = t.Merchant?.Name,
        Type = t.Type.ToString(),
        Status = t.Status.ToString(),
        Amount = t.Amount,
        BalanceAfter = t.BalanceAfter,
        Reference = t.Reference,
        IdempotencyKey = t.IdempotencyKey,
        RelatedTxnId = t.RelatedTxnId,
        Description = t.Description,
        CustomData = t.CustomData,
        CreatedAt = t.CreatedAt
    };
}
