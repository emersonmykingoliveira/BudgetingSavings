using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class SavingGoalsService(ApiDbContext db) : ISavingGoalsService
    {
        public Task<SavingGoal> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteSavingGoalAsync(Guid accountId, Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSavingGoalAsync(accountId, id, cancellationToken);

            if (savingGoal is not null)
            {
                db.SavingGoals.Remove(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            //todo: handle not found case
        }

        public async Task<List<SavingGoal>> GetAllSavingGoalsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.Where(s => s.AccountId == accountId).ToListAsync(cancellationToken);
        }

        public async Task<SavingGoal> GetSavingGoalAsync(Guid accountId, Guid id, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.FirstOrDefaultAsync(s => s.AccountId == accountId && s.Id == id, cancellationToken) ?? new SavingGoal();
        }

        public Task<SavingGoal> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
