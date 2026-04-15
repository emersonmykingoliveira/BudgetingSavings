using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BudgetingSavings.API.Services
{
    public class AccountService(ApiDbContext db, IValidator<CreateAccountRequest> createValidator) : IAccountService
    {
        public async Task<Result<AccountResponse>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result<AccountResponse>.Fail("Customer does not exist.");

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
                return Result<AccountResponse>.Fail($"Customer already has a {request.AccountType} account.");

            await db.Accounts.AddAsync(account, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result<AccountResponse>.Success(MapAccountResponse(account));
        }

        public async Task<Result> DeleteAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(id, cancellationToken);

            if (account is null)
                return Result.Fail("Account does not exist.");

            if (account.Balance > 0)
                return Result.Fail("Cannot delete an account that still contains money.");

            var hasTransactions = await db.Transactions.AnyAsync(t => t.AccountId == id, cancellationToken);
            if (hasTransactions)
                return Result.Fail("Cannot delete an account with transaction history.");

            db.Accounts.Remove(account);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<AccountResponse>> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var account = await GetSpecificAccountAsync(id, cancellationToken);

            if (account is null)
                return Result<AccountResponse>.Fail("Account does not exist.");

            return Result<AccountResponse>.Success(MapAccountResponse(account));
        }

        private async Task<Account?> GetSpecificAccountAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<Result<List<AccountResponse>>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            var accounts = await db.Accounts.ToListAsync(cancellationToken);
            return Result<List<AccountResponse>>.Success(accounts.Select(MapAccountResponse).ToList());
        }

        public async Task<Result<List<AccountResponse>>> GetAllAccountsForCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!customerExists)
                return Result<List<AccountResponse>>.Fail("Customer does not exist.");

            var accounts = await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync(cancellationToken);
            return Result<List<AccountResponse>>.Success(accounts.Select(MapAccountResponse).ToList());
        }

        public async Task<Result> UpdateAccountBalanceAsync(Guid id, decimal amount, CancellationToken cancellationToken, bool saveChanges = true)
        {
            if (amount == 0)
                return Result.Fail("Amount must be different than zero.");

            var account = await db.Accounts.FindAsync([id], cancellationToken);
            if (account is null)
                return Result.Fail("Account does not exist.");

            if (account.Balance + amount < 0)
                return Result.Fail("Insufficient balance.");

            account.Balance += amount;
            account.LastTransactionDate = DateTime.UtcNow;

            if (saveChanges)
                await db.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }

        private async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken cancellationToken)
        {
            var number = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            if (await db.Accounts.AnyAsync(a => a.AccountNumber == number, cancellationToken))
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
