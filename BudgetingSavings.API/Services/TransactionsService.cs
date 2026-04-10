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

            var transaction = new Transaction
            {
                AccountId = request.AccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Account = await accountsService.GetAccountAsync(request.AccountId, cancellationToken),
                Date = transactionDate
            };

            await db.Transactions.AddAsync(transaction, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await accountsService.UpdateAccountBalanceAsync(request.AccountId, request.Amount, transactionDate, cancellationToken);
            return transaction;
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
