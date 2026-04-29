using CardManagement.Api.Data;
using CardManagement.Api.Infrastructure;
using CardManagement.Api.Models.DTOs.Wallets;
using CardManagement.Api.Models.Entities;
using CardManagement.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardManagement.Api.Services;

public sealed class WalletService : IWalletService
{
    private readonly CardManagementDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IKafkaProducer _kafka;
    private readonly ILogger<WalletService> _logger;

    public WalletService(CardManagementDbContext db, ITenantContext tenant, IKafkaProducer kafka, ILogger<WalletService> logger)
    {
        _db = db;
        _tenant = tenant;
        _kafka = kafka;
        _logger = logger;
    }

    public async Task<WalletResponse> GetByCardIdAsync(string cardId, CancellationToken ct = default)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.CardId == cardId, ct)
            ?? throw new KeyNotFoundException($"Wallet for card '{cardId}' not found.");

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> LoadAsync(string cardId, LoadWalletRequest request, CancellationToken ct = default)
    {
        // Idempotency check
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _db.Transactions
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey, ct);
            if (existing is not null)
            {
                var existingWallet = await _db.Wallets.FirstOrDefaultAsync(w => w.CardId == cardId, ct);
                return MapToResponse(existingWallet!);
            }
        }

        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.CardId == cardId, ct)
            ?? throw new KeyNotFoundException($"Wallet for card '{cardId}' not found.");

        // Check card is active
        var card = await _db.Cards.Include(c => c.CardType).FirstOrDefaultAsync(c => c.Id == cardId, ct)
            ?? throw new KeyNotFoundException($"Card '{cardId}' not found.");

        if (card.Status != CardStatus.Active)
            throw new InvalidOperationException($"Cannot load wallet — card status is '{card.Status}'.");

        // Check max balance from card type
        if (card.CardType?.MaxBalance > 0 && wallet.Balance + request.Amount > card.CardType.MaxBalance)
            throw new InvalidOperationException($"Load would exceed max balance of {card.CardType.MaxBalance}.");

        wallet.Balance += request.Amount;
        wallet.Version++;

        var txn = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            WalletId = wallet.Id,
            Type = TransactionType.Load,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            BalanceAfter = wallet.Balance,
            Reference = request.Reference,
            IdempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString(),
            Description = request.Description,
            CustomData = request.CustomData,
            CreatedAt = DateTime.UtcNow
        };

        _db.Transactions.Add(txn);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Wallet {WalletId} loaded {Amount}, new balance {Balance}", wallet.Id, request.Amount, wallet.Balance);
        await _kafka.PublishWalletEventAsync("wallet.loaded", new { wallet.Id, wallet.CardId, request.Amount, wallet.Balance }, ct);

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> SpendAsync(string cardId, SpendWalletRequest request, CancellationToken ct = default)
    {
        // Idempotency check
        var existingTxn = await _db.Transactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey, ct);
        if (existingTxn is not null)
        {
            var w = await _db.Wallets.FirstOrDefaultAsync(wl => wl.CardId == cardId, ct);
            return MapToResponse(w!);
        }

        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.CardId == cardId, ct)
            ?? throw new KeyNotFoundException($"Wallet for card '{cardId}' not found.");

        var card = await _db.Cards.Include(c => c.CardType).FirstOrDefaultAsync(c => c.Id == cardId, ct)
            ?? throw new KeyNotFoundException($"Card '{cardId}' not found.");

        if (card.Status != CardStatus.Active)
            throw new InvalidOperationException($"Cannot spend — card status is '{card.Status}'.");

        // Reset daily/monthly counters if past reset date
        var now = DateTime.UtcNow;
        if (wallet.DailyResetAt.HasValue && now >= wallet.DailyResetAt.Value)
        {
            wallet.DailySpent = 0;
            wallet.DailyResetAt = now.Date.AddDays(1);
        }
        if (wallet.MonthlyResetAt.HasValue && now >= wallet.MonthlyResetAt.Value)
        {
            wallet.MonthlySpent = 0;
            wallet.MonthlyResetAt = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        }

        // Check balance
        if (wallet.Balance < request.Amount)
            throw new InvalidOperationException($"Insufficient balance. Available: {wallet.Balance}, requested: {request.Amount}.");

        // Check daily limit
        if (card.CardType?.DailySpendLimit > 0 && wallet.DailySpent + request.Amount > card.CardType.DailySpendLimit)
            throw new InvalidOperationException($"Daily spend limit of {card.CardType.DailySpendLimit} would be exceeded.");

        // Check monthly limit
        if (card.CardType?.MonthlySpendLimit > 0 && wallet.MonthlySpent + request.Amount > card.CardType.MonthlySpendLimit)
            throw new InvalidOperationException($"Monthly spend limit of {card.CardType.MonthlySpendLimit} would be exceeded.");

        wallet.Balance -= request.Amount;
        wallet.DailySpent += request.Amount;
        wallet.MonthlySpent += request.Amount;
        wallet.Version++;

        var txn = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            WalletId = wallet.Id,
            MerchantId = request.MerchantId,
            Type = TransactionType.Spend,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            BalanceAfter = wallet.Balance,
            Reference = request.Reference,
            IdempotencyKey = request.IdempotencyKey,
            Description = request.Description,
            CustomData = request.CustomData,
            CreatedAt = DateTime.UtcNow
        };

        _db.Transactions.Add(txn);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Wallet {WalletId} spent {Amount}, new balance {Balance}", wallet.Id, request.Amount, wallet.Balance);
        await _kafka.PublishWalletEventAsync("wallet.spent", new { wallet.Id, wallet.CardId, request.Amount, wallet.Balance, request.MerchantId }, ct);

        return MapToResponse(wallet);
    }

    public async Task<WalletResponse> RefundAsync(string cardId, RefundWalletRequest request, CancellationToken ct = default)
    {
        // Idempotency check
        var existingTxn = await _db.Transactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey, ct);
        if (existingTxn is not null)
        {
            var w = await _db.Wallets.FirstOrDefaultAsync(wl => wl.CardId == cardId, ct);
            return MapToResponse(w!);
        }

        // Validate original transaction
        var originalTxn = await _db.Transactions.FindAsync([request.OriginalTransactionId], ct)
            ?? throw new KeyNotFoundException($"Original transaction '{request.OriginalTransactionId}' not found.");

        if (originalTxn.Type != TransactionType.Spend)
            throw new InvalidOperationException("Refunds can only be issued against spend transactions.");

        if (originalTxn.Status == TransactionStatus.Reversed)
            throw new InvalidOperationException("Transaction has already been reversed.");

        if (request.Amount > originalTxn.Amount)
            throw new InvalidOperationException($"Refund amount {request.Amount} exceeds original transaction amount {originalTxn.Amount}.");

        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.CardId == cardId, ct)
            ?? throw new KeyNotFoundException($"Wallet for card '{cardId}' not found.");

        wallet.Balance += request.Amount;
        wallet.Version++;

        // Reverse daily/monthly spent if applicable
        wallet.DailySpent = Math.Max(0, wallet.DailySpent - request.Amount);
        wallet.MonthlySpent = Math.Max(0, wallet.MonthlySpent - request.Amount);

        var txn = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            OrgId = _tenant.OrgId,
            WalletId = wallet.Id,
            MerchantId = originalTxn.MerchantId,
            Type = TransactionType.Refund,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            BalanceAfter = wallet.Balance,
            Reference = request.Reference,
            IdempotencyKey = request.IdempotencyKey,
            RelatedTxnId = request.OriginalTransactionId,
            Description = request.Description,
            CustomData = request.CustomData,
            CreatedAt = DateTime.UtcNow
        };

        // Mark full refund as reversed
        if (request.Amount >= originalTxn.Amount)
            originalTxn.Status = TransactionStatus.Reversed;

        _db.Transactions.Add(txn);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Wallet {WalletId} refunded {Amount}, new balance {Balance}", wallet.Id, request.Amount, wallet.Balance);
        await _kafka.PublishWalletEventAsync("wallet.refunded", new { wallet.Id, wallet.CardId, request.Amount, wallet.Balance, OriginalTxnId = request.OriginalTransactionId }, ct);

        return MapToResponse(wallet);
    }

    private static WalletResponse MapToResponse(Wallet w) => new()
    {
        Id = w.Id,
        CardId = w.CardId,
        Balance = w.Balance,
        Currency = w.Currency,
        DailySpent = w.DailySpent,
        MonthlySpent = w.MonthlySpent,
        DailyResetAt = w.DailyResetAt,
        MonthlyResetAt = w.MonthlyResetAt,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };
}
