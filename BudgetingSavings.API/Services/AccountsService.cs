using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class AccountsService(ApiDbContext db) : IAccountsService
    {
        public async Task<Account> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = GenerateUniqueAccountNumber(),
                AccountType = request.AccountType,
                Currency = request.Currency,
                Owner = request.Owner,
                Balance = 0m
            };

            await db.Accounts.AddAsync(account, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return account;
        }

        public async Task<Account> GetAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Accounts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new Account();
        }

        public async Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            return await db.Accounts.ToListAsync(cancellationToken);
        }

        private string GenerateUniqueAccountNumber()
        {
            //generate a unique account number, in the format "1234.56.78901"
            //ensure that the generated account number is unique by checking against the database

               

        }
    }
}
