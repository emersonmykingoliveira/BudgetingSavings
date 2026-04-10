using BudgetingSavings.API.Infrastructure.Entities;

namespace BudgetingSavings.API.Services
{
    public interface IAccountsService
    {
        Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<Account> GetAccountAsync(CancellationToken cancellationToken, Guid id);
    }
}
