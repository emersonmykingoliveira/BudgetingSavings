using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface ITransactionService
    {
        Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
        Task<List<Result<TransactionResponse>>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken);
        Task<Result<TransactionResponse>> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<TransferResponse>> CreateTransferAsync(CreateTransferRequest request, CancellationToken cancellationToken);
    }
}
