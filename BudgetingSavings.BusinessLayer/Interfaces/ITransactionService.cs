using BudgetingSavings.BusinessLayer.Infrastructure.Entities;
using BudgetingSavings.BusinessLayer.Models.Requests;
using BudgetingSavings.BusinessLayer.Models.Responses;

namespace BudgetingSavings.BusinessLayer.Services
{
    public interface ITransactionService
    {
        Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
        Task<List<TransactionResponse>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken);
        Task<TransactionResponse> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken);
    }
}
