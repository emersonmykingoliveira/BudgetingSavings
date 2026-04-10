using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class TransactionsService(ApiDbContext db, IAccountsService accountsService) : ITransactionsService
    {
        public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var transactionDate = DateTime.Now;

            await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var account = await accountsService.GetAccountAsync(request.CustomerId, request.AccountId, cancellationToken);

                if (account is null || account.Id == Guid.Empty)
                    throw new ArgumentException($"Account with Id {request.AccountId} not found.");

                var transaction = new Transaction
                {
                    AccountId = request.AccountId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    Account = account,
                    Date = transactionDate
                };

                await db.Transactions.AddAsync(transaction, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await accountsService.UpdateAccountBalanceAsync(request.CustomerId, request.AccountId, request.Amount, transactionDate, cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
                return transaction;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            return await db.Transactions.Where(s => s.AccountId == accountId).ToListAsync(cancellationToken);
        }

        public async Task<Transaction> GetTransactionAsync(Guid accountId, Guid id, CancellationToken cancellationToken)
        {
            return await db.Transactions.FirstOrDefaultAsync(s => s.Id == id && s.AccountId == accountId, cancellationToken) ?? new Transaction();
        }
    }
}
