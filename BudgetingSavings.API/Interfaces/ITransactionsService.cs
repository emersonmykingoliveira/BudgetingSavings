using BudgetingSavings.API.Infrastructure.Entities;

namespace BudgetingSavings.API.Services
{
    public interface ITransactionsService
    {
        Task<List<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken);
        Task<Transaction> GetTransactionAsync(Guid id, CancellationToken cancellationToken);
    }
}
