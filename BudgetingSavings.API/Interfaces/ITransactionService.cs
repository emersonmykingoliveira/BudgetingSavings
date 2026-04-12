using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
        Task<List<TransactionResponse>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken);
        Task<TransactionResponse> GetTransactionAsync(Guid id, Guid accountId, CancellationToken cancellationToken);
    }
}
