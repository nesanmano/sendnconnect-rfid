using CardManagement.Api.Models.DTOs.Common;
using CardManagement.Api.Models.DTOs.Transactions;

namespace CardManagement.Api.Services;

public interface ITransactionService
{
    Task<TransactionResponse> GetByIdAsync(string id, CancellationToken ct = default);
    Task<PagedResponse<TransactionResponse>> ListAsync(TransactionQueryParams query, CancellationToken ct = default);
}
