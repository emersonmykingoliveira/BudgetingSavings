using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class TransactionService(ApiDbContext db, IAccountService accountsService, IRewardService rewardService) : ITransactionService
    {
        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = new Transaction
                {
                    AccountId = request.AccountId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    TransactionType = request.TransactionType,
                    TransactionCategory = request.TransactionCategory,
                    TransactionDateTime = DateTime.Now
                };

                await db.Transactions.AddAsync(transaction, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await accountsService.UpdateAccountBalanceAsync(request.AccountId, request.Amount, cancellationToken);
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
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                TransactionType = transaction.TransactionType,
                TransactionCategory = transaction.TransactionCategory,
                TransactionDateTime = transaction.TransactionDateTime
            };
        }
    }
}
