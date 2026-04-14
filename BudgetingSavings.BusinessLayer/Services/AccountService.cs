using BudgetingSavings.BusinessLayer.Infrastructure.Data;
using BudgetingSavings.BusinessLayer.Infrastructure.Entities;
using BudgetingSavings.BusinessLayer.Models.Requests;
using BudgetingSavings.BusinessLayer.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BudgetingSavings.BusinessLayer.Services
{
    public class AccountService(ApiDbContext db, IValidator<CreateAccountRequest> createValidator) : IAccountService
    {
        public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                throw new ArgumentException("Customer does not exist.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                AccountNumber = await GenerateUniqueAccountNumberAsync(cancellationToken),
                AccountType = request.AccountType,
                Currency = request.Currency,
                CustomerId = request.CustomerId,
                Balance = 0m
            };

            var hasSameType = await db.Accounts.AnyAsync(a => a.CustomerId == request.CustomerId
                                                        && a.AccountType == request.AccountType, cancellationToken);

            if (hasSameType)
                throw new ArgumentException($"Customer already has a {request.AccountType} account.");

            await db.Accounts.AddAsync(account, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return MapAccountResponse(account);
        }

        public async Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(id, cancellationToken);

            if (account.Balance > 0)
                throw new ArgumentException("Cannot delete an account that still contains money.");

            var hasTransactions = await db.Transactions.AnyAsync(t => t.AccountId == id, cancellationToken);
            if (hasTransactions)
                throw new ArgumentException("Cannot delete an account with transaction history.");

            db.Accounts.Remove(account);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<AccountResponse> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(id, cancellationToken);

            return MapAccountResponse(account);
        }

        private async Task<Account> GetSpecificAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (account is null)
                throw new ArgumentException("Account does not exist.");

            return account;
        }

        public async Task<List<AccountResponse>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            var accounts = await db.Accounts.ToListAsync(cancellationToken);
            return accounts.Select(MapAccountResponse).ToList();
        }

        public async Task<List<AccountResponse>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!customerExists)
                throw new ArgumentException("Customer does not exist.");

            var accounts = await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync(cancellationToken);
            return accounts.Select(MapAccountResponse).ToList();
        }

        public async Task UpdateAccountBalanceAsync(Guid id, decimal amount, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(id, cancellationToken);

            if (amount == 0)
                throw new ArgumentException("Amount must be different than zero.");

            if (account.Balance + amount < 0)
                throw new ArgumentException("Insufficient balance.");

            account.Balance += amount;
            account.LastTransactionDate = DateTime.UtcNow;
            db.Accounts.Update(account);
            await db.SaveChangesAsync(cancellationToken);
            
        }

        private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
        {
            int finalNumber = Random.Shared.Next(1000, 10000);

            var number = string.Concat("********", finalNumber);

            if(await db.Accounts.AnyAsync(a => a.AccountNumber == number, cancellationToken))
                return await GenerateUniqueAccountNumberAsync(cancellationToken);

            return number;
        }

        private AccountResponse MapAccountResponse(Account? account)
        {
            if(account is null) return new AccountResponse();

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
