using BudgetingSavings.API.Infrastructure.Entities;

namespace BudgetingSavings.API.Services
{
    public interface ITransactionsService
    {
        Task<List<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken);
    }
}
