using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class TransactionsService(ApiDbContext db) : ITransactionsService
    {
        public async Task<List<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken)
        {
            return await db.Transactions.ToListAsync(cancellationToken);
        }
    }
}
