using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Services
{
    public interface IAccountsService
    {
        Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<Account> GetAccountAsync(Guid id, CancellationToken cancellationToken);
        Task<Account> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    }
}
