using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IAccountsService
    {
        Task<List<AccountResponse>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<AccountResponse> GetAccountAsync(Guid customerId, Guid id1, CancellationToken cancellationToken);
        Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
        Task DeleteAccountAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
        Task UpdateAccountBalanceAsync(Guid customerId, Guid id, decimal amount, DateTime transactionDate, CancellationToken cancellationToken);
        Task<List<AccountResponse>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
