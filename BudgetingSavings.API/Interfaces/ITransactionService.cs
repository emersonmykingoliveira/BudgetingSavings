using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Services
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
        Task<List<Transaction>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken);
        Task<Transaction> GetTransactionAsync(Guid id, Guid accountId, CancellationToken cancellationToken);
    }
}
