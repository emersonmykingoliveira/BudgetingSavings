using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class SavingGoalService(ApiDbContext db) : ISavingGoalService
    {
        public async Task<SavingGoal> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                TargetAmount = request.TargetAmount,
                StartDate = DateTime.Now,    
                TargetDate = request.TargetDate,
                CustomerId = request.CustomerId
            };

            await db.SavingGoals.AddAsync(savingGoal, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return savingGoal;
        }

        public async Task DeleteSavingGoalAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSavingGoalAsync(customerId, id, cancellationToken);

            if (savingGoal is not null)
            {
                db.SavingGoals.Remove(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            //todo: handle not found case
        }

        public async Task<List<SavingGoal>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
        }

        public async Task<SavingGoal> GetSavingGoalAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == customerId, cancellationToken) ?? new SavingGoal();
        }

        public async Task<SavingGoal> UpdateSavingGoalAsync(Guid id, UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSavingGoalAsync(request.CustomerId, id, cancellationToken);
            
            if (savingGoal is not null)
            {
                savingGoal.Name = request.Name;
                savingGoal.TargetAmount = request.TargetAmount;
                savingGoal.TargetDate = request.TargetDate;

                db.SavingGoals.Update(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            return savingGoal ?? new SavingGoal();
        }
    }
}
