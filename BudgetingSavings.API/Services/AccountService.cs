using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BudgetingSavings.API.Services
{
    public class AccountService(ApiDbContext db) : IAccountService
    {
        public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                AccountNumber = await GenerateUniqueAccountNumberAsync(cancellationToken),
                AccountType = request.AccountType,
                Currency = request.Currency,
                CustomerId = request.CustomerId,
                Balance = 0m
            };

            await db.Accounts.AddAsync(account, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return MapAccountResponse(account);
        }

        public async Task DeleteAccountAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(customerId, id, cancellationToken);

            if (account is not null)
            {
                db.Accounts.Remove(account);
                await db.SaveChangesAsync(cancellationToken);
            }
            //todo: handle not found case
        }

        public async Task<AccountResponse> GetAccountAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(customerId, id, cancellationToken);
            return MapAccountResponse(account);
        }

        private async Task<Account> GetSpecificAccountAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            return await db.Accounts.FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == customerId, cancellationToken) ?? new Account();
        }

        public async Task<List<AccountResponse>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            var accounts = await db.Accounts.ToListAsync(cancellationToken);
            return accounts.Select(MapAccountResponse).ToList();
        }

        public async Task<List<AccountResponse>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var accounts = await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync(cancellationToken);
            return accounts.Select(MapAccountResponse).ToList();
        }

        public async Task UpdateAccountBalanceAsync(Guid customerId, Guid id, decimal amount, DateTime transactionDate, CancellationToken cancellationToken)
        {
            var account = await db.Accounts.FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == customerId, cancellationToken);

            if (account is not null)
            {
                account.Balance += amount;
                account.LastTransactionDate = transactionDate;
                db.Accounts.Update(account);
                await db.SaveChangesAsync(cancellationToken);
            }

            //TODO: Handle case when account is not found (e.g., throw an exception or return a result indicating failure)
        }

        private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
        {
            int finalNumber = Random.Shared.Next(1000, 10000);

            var number = string.Concat("********", finalNumber);

            if(await db.Accounts.AnyAsync(a => a.AccountNumber == number, cancellationToken))
                return await GenerateUniqueAccountNumberAsync(cancellationToken);

            return number;
        }

        private AccountResponse MapAccountResponse(Account account)
        {
            return new AccountResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                AccountType = account.AccountType,
                Balance = account.Balance,
                Currency = account.Currency,
                CreatedDate = account.CreatedDate,
                LastTransactionDate = account.LastTransactionDate,
                CustomerId = account.CustomerId
            };
        }
    }
}
