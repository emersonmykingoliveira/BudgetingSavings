using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class TransactionService(ApiDbContext db, 
                                    IAccountService accountService, 
                                    IRewardService rewardService,
                                    IValidator<CreateTransactionRequest> createValidator) : ITransactionService
    {
        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

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
                await db.SaveChangesAsync(cancellationToken);
                await accountService.UpdateAccountBalanceAsync(request.AccountId,
                                                                request.TransactionType == TransactionType.Debit ? -request.Amount : request.Amount, 
                                                                cancellationToken);
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
            return await db.Transactions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new Transaction();
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
            if (request.AccountOriginId == request.AccountDestinyId)
                throw new ArgumentException("Origin and destination accounts cannot be the same.");

            if (request.Amount <= 0)
                throw new ArgumentException("Transfer amount must be greater than zero.");

            await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Debit Origin
                await accountService.UpdateAccountBalanceAsync(request.AccountOriginId, -request.Amount, cancellationToken);
                var accountOrigin = await db.Accounts.FirstAsync(a => a.Id == request.AccountOriginId, cancellationToken);

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

                // Credit Destiny
                await accountService.UpdateAccountBalanceAsync(request.AccountDestinyId, request.Amount, cancellationToken);
                var accountDestiny = await db.Accounts.FirstAsync(a => a.Id == request.AccountDestinyId, cancellationToken);

                var destinyTransaction = new Transaction
                {
                    AccountId = request.AccountDestinyId,
                    CustomerId = accountDestiny.CustomerId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    TransactionType = TransactionType.Credit,
                    TransactionCategory = TransactionCategory.General,
                    TransactionDateTime = DateTime.UtcNow
                };
                await db.Transactions.AddAsync(destinyTransaction, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                return new TransferResponse
                {
                    AccountOriginId = request.AccountOriginId,
                    AccountDestinyId = request.AccountDestinyId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Date = originTransaction.TransactionDateTime
                };
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
