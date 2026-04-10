using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BudgetingSavings.API.Services
{
    public class AccountsService(ApiDbContext db) : IAccountsService
    {
        public async Task<Account> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                AccountNumber = await GenerateUniqueAccountNumberAsync(cancellationToken),
                AccountType = request.AccountType,
                Currency = request.Currency,
                Owner = request.Owner,
                Balance = 0m
            };

            await db.Accounts.AddAsync(account, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return account;
        }

        public async Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            var account = await GetAccountAsync(id, cancellationToken);

            if (account is not null)
            {
                db.Accounts.Remove(account);
                await db.SaveChangesAsync(cancellationToken);
            }
            //todo: handle not found case
        }

        public async Task<Account> GetAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Accounts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new Account();
        }

        public async Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            return await db.Accounts.ToListAsync(cancellationToken);
        }

        public async Task UpdateAccountBalanceAsync(Guid accountId, decimal amount, DateTime transactionDate, CancellationToken cancellationToken)
        {
            var account = await db.Accounts.FirstOrDefaultAsync(s => s.Id == accountId);

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
    }
}
