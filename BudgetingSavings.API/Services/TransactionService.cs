using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class TransactionService(ApiDbContext db, IValidator<CreateTransactionRequest> createValidator) : ITransactionService
    {
        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                throw new ArgumentException("Customer does not exist.");

            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            if (account is null)
                throw new ArgumentException("Account does not exist.");

            if (account.CustomerId != request.CustomerId)
                throw new ArgumentException("Account does not belong to the customer.");

            if (account.Currency != request.Currency)
                throw new ArgumentException("Transaction currency must match account currency.");

            return await TransactionHandler(request, account, cancellationToken);
        }

        private async Task<TransactionResponse> TransactionHandler(CreateTransactionRequest request, Account account, CancellationToken cancellationToken)
        {
            await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = new Transaction
                {
                    AccountId = request.AccountId,
                    CustomerId = request.CustomerId,
                    Amount = request.TransactionType == TransactionType.Debit ? -request.Amount : request.Amount,
                    Currency = request.Currency,
                    TransactionType = request.TransactionType,
                    TransactionCategory = request.TransactionCategory,
                    TransactionDateTime = DateTime.UtcNow
                };

                await db.Transactions.AddAsync(transaction, cancellationToken);
 
                UpdateAccountBalance(account, request.TransactionType == TransactionType.Debit ? -request.Amount : request.Amount);

                if (request.TransactionType == TransactionType.Debit)
                    await HandleRoundUpToSavingsAsync(request, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
                await HandleRewardAsync(request, cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
                return MapTransactionResponse(transaction);
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task HandleRoundUpToSavingsAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var roundUpAmount = Math.Ceiling(request.Amount) - request.Amount;
            if (roundUpAmount <= 0) return;

            var currentAccount = await db.Accounts.FindAsync([request.AccountId], cancellationToken);
            if (currentAccount is null || currentAccount.AccountType == AccountType.Savings) return;

            if (currentAccount.Balance < roundUpAmount) return;

            var savingsAccount = await db.Accounts.FirstOrDefaultAsync(a => a.CustomerId == request.CustomerId
                                                                            && a.AccountType == AccountType.Savings, cancellationToken);
            if (savingsAccount is null) return;

            UpdateAccountBalance(currentAccount, -roundUpAmount);

            var debitTransaction = new Transaction
            {
                AccountId = request.AccountId,
                CustomerId = request.CustomerId,
                Amount = -roundUpAmount,
                Currency = request.Currency,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Savings,
                TransactionDateTime = DateTime.UtcNow
            };
            await db.Transactions.AddAsync(debitTransaction, cancellationToken);

            UpdateAccountBalance(savingsAccount, roundUpAmount);

            var creditTransaction = new Transaction
            {
                AccountId = savingsAccount.Id,
                CustomerId = request.CustomerId,
                Amount = roundUpAmount,
                Currency = request.Currency,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings,
                TransactionDateTime = DateTime.UtcNow
            };
            await db.Transactions.AddAsync(creditTransaction, cancellationToken);
        }

        private async Task HandleRewardAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var rewardRequest = new CreateRewardRequest
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                TransactionType = request.TransactionType,
                TransactionCategory = request.TransactionCategory
            };

            await rewardService.RewardHandlerAsync(rewardRequest, cancellationToken);
        }

        public async Task<List<TransactionResponse>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken);

            if (!accountExists)
                throw new ArgumentException("Account does not exist.");

            var transactions = await db.Transactions.Where(s => s.AccountId == accountId).ToListAsync(cancellationToken);
            return transactions.Select(MapTransactionResponse).ToList();
        }

        public async Task<TransactionResponse> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var transaction = await GetSpecificTransactionAsync(id, cancellationToken);
            return MapTransactionResponse(transaction);
        }

        private async Task<Transaction> GetSpecificTransactionAsync(Guid id, CancellationToken cancellationToken)
        {
            var transaction = await db.Transactions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if(transaction is null)
                throw new ArgumentException("Transaction does not exist.");

            return transaction;
        }

        private TransactionResponse MapTransactionResponse(Transaction? transaction)
        {
            if (transaction is null) return new TransactionResponse();
            return new TransactionResponse
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                CustomerId = transaction.CustomerId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                TransactionType = transaction.TransactionType,
                TransactionCategory = transaction.TransactionCategory,
                TransactionDateTime = transaction.TransactionDateTime
            };
        }

        public async Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken)
        {
            if (request.AccountOriginId == request.AccountDestinationId)
                throw new ArgumentException("Origin and destination accounts cannot be the same.");

            if (request.Amount <= 0)
                throw new ArgumentException("Transfer amount must be greater than zero.");

            await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var accountOrigin = await db.Accounts.FirstAsync(a => a.Id == request.AccountOriginId, cancellationToken);

                var accountDestination = await db.Accounts.FirstAsync(a => a.Id == request.AccountDestinationId, cancellationToken);

                if (accountOrigin?.Currency != request.Currency || accountDestination?.Currency != request.Currency)
                    throw new ArgumentException("Transfer currency must match both account currencies.");

                await DebitOriginAccountHandler(request, accountOrigin, cancellationToken);
                await CreditDestinationAccountHandler(request, accountDestination, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                return new TransferResponse
                {
                    AccountOriginId = request.AccountOriginId,
                    AccountDestinyId = request.AccountDestinationId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Date = DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task CreditDestinationAccountHandler(TransferRequest request, Account accountDestination, CancellationToken cancellationToken)
        {
            if (accountDestination is null)
                throw new ArgumentException("Destination account does not exist.");

            UpdateAccountBalance(accountDestination, request.Amount);

            var destinyTransaction = new Transaction
            {
                AccountId = request.AccountDestinationId,
                CustomerId = accountDestination.CustomerId,
                Amount = request.Amount,
                Currency = request.Currency,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.General,
                TransactionDateTime = DateTime.UtcNow
            };
            await db.Transactions.AddAsync(destinyTransaction, cancellationToken);
        }

        private async Task DebitOriginAccountHandler(TransferRequest request, Account accountOrigin, CancellationToken cancellationToken)
        {
            if (accountOrigin is null)
                throw new ArgumentException("Origin account does not exist.");

            if (accountOrigin.Balance < request.Amount)
                throw new ArgumentException("Insufficient balance for transfer.");

            UpdateAccountBalance(accountOrigin, -request.Amount);

            var originTransaction = new Transaction
            {
                AccountId = request.AccountOriginId,
                CustomerId = accountOrigin.CustomerId,
                Amount = -request.Amount,
                Currency = request.Currency,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.General,
                TransactionDateTime = DateTime.UtcNow
            };
            await db.Transactions.AddAsync(originTransaction, cancellationToken);
        }

        private static void UpdateAccountBalance(Account account, decimal amount)
        {
            if (amount == 0)
                throw new ArgumentException("Amount must be different from zero.");

            if (account.Balance + amount < 0)
                throw new ArgumentException("Insufficient balance.");

            account.Balance += amount;
            account.LastTransactionDate = DateTime.UtcNow;
        }
    }
}
