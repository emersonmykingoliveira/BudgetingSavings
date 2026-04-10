using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Services
{
    public interface IAccountsService
    {
        Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<Account> GetAccountAsync(Guid customerId, Guid id1, CancellationToken cancellationToken);
        Task<Account> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
        Task DeleteAccountAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
        Task UpdateAccountBalanceAsync(Guid customerId, Guid id, decimal amount, DateTime transactionDate, CancellationToken cancellationToken);
        Task<List<Account>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
