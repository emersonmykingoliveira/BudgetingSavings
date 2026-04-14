using BudgetingSavings.BusinessLayer.Infrastructure.Entities;
using BudgetingSavings.BusinessLayer.Models.Requests;
using BudgetingSavings.BusinessLayer.Models.Responses;

namespace BudgetingSavings.BusinessLayer.Services
{
    public interface IAccountService
    {
        Task<List<AccountResponse>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<AccountResponse> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
        Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateAccountBalanceAsync(Guid id, decimal amount, CancellationToken cancellationToken);
        Task<List<AccountResponse>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
