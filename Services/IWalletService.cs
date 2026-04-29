using CardManagement.Api.Models.DTOs.Wallets;

namespace CardManagement.Api.Services;

public interface IWalletService
{
    Task<WalletResponse> GetByCardIdAsync(string cardId, CancellationToken ct = default);
    Task<WalletResponse> LoadAsync(string cardId, LoadWalletRequest request, CancellationToken ct = default);
    Task<WalletResponse> SpendAsync(string cardId, SpendWalletRequest request, CancellationToken ct = default);
    Task<WalletResponse> RefundAsync(string cardId, RefundWalletRequest request, CancellationToken ct = default);
}
