using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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
                var updateResult = await UpdateAccountBalanceAsync(account, request.TransactionType == TransactionType.Credit ? request.Amount : -request.Amount, request.TransactionCategory, cancellationToken);

                if (updateResult.Value is null || updateResult.IsFailure)
                    return Result<TransactionResponse>.Fail(updateResult.Error ?? "An error occurred while updating the account balance.");

                if (request.TransactionType == TransactionType.Debit)
                    await HandleRoundUpToSavingsAsync(request, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
                await ApplyRewardsForTransactionAsync(request, cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
                return Result<TransactionResponse>.Success(MapTransactionResponse(updateResult.Value));
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                return Result<TransactionResponse>.Fail(ex.Message);
            }
        }

        private async Task<Result> HandleRoundUpToSavingsAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var roundUpAmount = Math.Ceiling(request.Amount) - request.Amount;

            if (roundUpAmount <= 0) return Result.Success();

            var currentAccount = await db.Accounts.FindAsync([request.AccountId], cancellationToken);

            if (currentAccount is null)
                return Result.Fail("Current account not found.");

            if (currentAccount.Balance < roundUpAmount) 
                return Result.Success();

            var savingsAccount = await db.Accounts.FirstOrDefaultAsync(a => a.CustomerId == request.CustomerId
                                                                            && a.AccountType == AccountType.Savings, cancellationToken);
            if (savingsAccount is null)
                return Result.Fail("Savings account not found for the customer.");

            var debitResult = await UpdateAccountBalanceAsync(currentAccount, -roundUpAmount, TransactionCategory.Savings, cancellationToken);

            if (debitResult.IsFailure)
                return Result.Fail(debitResult.Error ?? "An error occurred while debiting the current account.");

            var creditResult = await UpdateAccountBalanceAsync(savingsAccount, roundUpAmount, TransactionCategory.Savings, cancellationToken);

            if (creditResult.IsFailure)
                return Result.Fail(creditResult.Error ?? "An error occurred while crediting the savings account.");

            return Result.Success();
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

        public async Task<Result<List<TransactionResponse>>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var accountExists = await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken);

            if (!accountExists)
                return Result<List<TransactionResponse>>.Fail("Account does not exist.");

            var transactions = await db.Transactions.Where(s => s.AccountId == accountId).ToListAsync(cancellationToken);
            return Result<List<TransactionResponse>>.Success(transactions.Select(MapTransactionResponse).ToList());
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

        private TransactionResponse MapTransactionResponse(Transaction transaction)
        {
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

                var debitResult = await UpdateAccountBalanceAsync(accountOrigin, -request.Amount, TransactionCategory.General, cancellationToken);

                if (debitResult.IsFailure)
                    return Result<TransferResponse>.Fail(debitResult.Error ?? "An error occurred while debiting the origin account.");

                var creditResult = await UpdateAccountBalanceAsync(accountDestination, request.Amount, TransactionCategory.General, cancellationToken);

                if (creditResult.IsFailure)
                    return Result<TransferResponse>.Fail(creditResult.Error ?? "An error occurred while crediting the destination account.");

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

        private async Task<Result<Transaction>> UpdateAccountBalanceAsync(Account account, decimal amount, TransactionCategory category, CancellationToken cancellationToken)
        {
            var destinyTransaction = new Transaction
            {
                AccountId = account.Id,
                CustomerId = account.CustomerId,
                Amount = amount,
                Currency = account.Currency,
                TransactionType = amount >= 0 ? TransactionType.Credit : TransactionType.Debit,
                TransactionCategory = category,
                TransactionDateTime = DateTime.UtcNow
            };
            await db.Transactions.AddAsync(destinyTransaction, cancellationToken);

            if (amount == 0)
                return Result<Transaction>.Fail("Amount must be non-zero.");

            if (account.Balance + amount < 0)
                return Result<Transaction>.Fail("Insufficient balance.");

            account.Balance += amount;
            account.LastTransactionDate = DateTime.UtcNow;

            return Result<Transaction>.Success(destinyTransaction);
        }
    }
}
