using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class AccountsService(ApiDbContext db) : IAccountsService
    {
        public async Task<Account> GetAccountAsync(CancellationToken cancellationToken, Guid id)
        {
            return await db.Accounts.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new Account();
        }

        public async Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken)
        {
            return await db.Accounts.ToListAsync(cancellationToken);
        }

    }
}
