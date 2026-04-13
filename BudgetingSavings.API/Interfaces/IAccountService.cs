using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IAccountService
    {
        Task<List<AccountResponse>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<AccountResponse> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
        Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateAccountBalanceAsync(Guid id, Guid customerId, decimal amount, CancellationToken cancellationToken);
        Task<List<AccountResponse>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
