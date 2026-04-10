using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class SavingGoalsService(ApiDbContext db) : ISavingGoalsService
    {
        public async Task<List<SavingGoal>> GetAllSavingGoalsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.Where(s => s.AccountId == accountId).ToListAsync(cancellationToken);
        }
    }
}
