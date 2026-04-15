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
    public class TransactionService(ApiDbContext db, 
                                    IRewardService rewardService,
                                    IValidator<CreateTransactionRequest> createValidator) : ITransactionService
    {
        public async Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result<TransactionResponse>.Fail("Customer does not exist.");

            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            if (account is null)
                return Result<TransactionResponse>.Fail("Account does not exist.");

            if (account.CustomerId != request.CustomerId)
                return Result<TransactionResponse>.Fail("Account does not belong to the customer.");

            if (account.Currency != request.Currency)
                return Result<TransactionResponse>.Fail("Transaction currency must match account currency.");

            return await TransactionHandler(request, account, cancellationToken);
        }

        private async Task<Result<TransactionResponse>> TransactionHandler(CreateTransactionRequest request, Account account, CancellationToken cancellationToken)
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
                await ApplyRewardsForTransactionAsync(request, cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
                return Result<TransactionResponse>.Success(MapTransactionResponse(transaction));
            }
            catch (ArgumentException ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                return Result<TransactionResponse>.Fail(ex.Message);
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

        private async Task ApplyRewardsForTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
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

        public async Task<List<Result<TransactionResponse>>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken);

            if (!accountExists)
                return [Result<TransactionResponse>.Fail("Account does not exist.")];

            var transactions = await db.Transactions.Where(s => s.AccountId == accountId).ToListAsync(cancellationToken);
            return transactions.Select(t => Result<TransactionResponse>.Success(MapTransactionResponse(t))).ToList();
        }

        public async Task<Result<TransactionResponse>> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var transaction = await GetSpecificTransactionAsync(id, cancellationToken);

            if (transaction is null)
                return Result<TransactionResponse>.Fail("Transaction does not exist.");

            return Result<TransactionResponse>.Success(MapTransactionResponse(transaction));
        }

        private async Task<Transaction?> GetSpecificTransactionAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Transactions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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

        public async Task<Result<TransferResponse>> CreateTransferAsync(CreateTransferRequest request, CancellationToken cancellationToken)
        {
            if (request.AccountOriginId == request.AccountDestinationId)
                return Result<TransferResponse>.Fail("Origin and destination accounts cannot be the same.");

            if (request.Amount <= 0)
                return Result<TransferResponse>.Fail("Transfer amount must be greater than zero.");

            await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var accountOrigin = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountOriginId, cancellationToken);

                var accountDestination = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountDestinationId, cancellationToken);

                if (accountOrigin is null)
                    return Result<TransferResponse>.Fail("Origin account does not exist.");

                if (accountDestination is null)
                    return Result<TransferResponse>.Fail("Destination account does not exist.");

                if (accountOrigin.Currency != request.Currency || accountDestination.Currency != request.Currency)
                    return Result<TransferResponse>.Fail("Transfer currency must match both account currencies.");

                if (accountOrigin.Balance < request.Amount)
                    return Result<TransferResponse>.Fail("Insufficient balance for transfer.");

                await DebitOriginAccountHandler(request, accountOrigin, cancellationToken);
                await CreditDestinationAccountHandler(request, accountDestination, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                return Result<TransferResponse>.Success(new TransferResponse
                {
                    Id = Guid.NewGuid(),
                    AccountDestinyId = request.AccountDestinationId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Date = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                return Result<TransferResponse>.Fail(ex.Message);
            }
        }

        private async Task CreditDestinationAccountHandler(CreateTransferRequest request, Account accountDestination, CancellationToken cancellationToken)
        {
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

        private async Task DebitOriginAccountHandler(CreateTransferRequest request, Account accountOrigin, CancellationToken cancellationToken)
        {
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
