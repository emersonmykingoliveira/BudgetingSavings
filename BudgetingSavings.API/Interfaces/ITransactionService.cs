using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
        Task<List<TransactionResponse>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken);
        Task<TransactionResponse> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken);
    }
}
