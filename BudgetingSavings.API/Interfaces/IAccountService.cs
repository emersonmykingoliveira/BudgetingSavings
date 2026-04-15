using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IAccountService
    {
        Task<List<Result<AccountResponse>>> GetAllAccountsAsync(CancellationToken cancellationToken);
        Task<Result<AccountResponse>> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
        Task<Result> DeleteAccountAsync(Guid id, CancellationToken cancellationToken);
        Task<Result> UpdateAccountBalanceAsync(Guid id, decimal amount, CancellationToken cancellationToken, bool saveChanges = true);
        Task<List<Result<AccountResponse>>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
