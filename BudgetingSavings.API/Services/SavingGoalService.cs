using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class SavingGoalService(ApiDbContext db) : ISavingGoalService
    {
        public async Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
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

            return MapSavingGoalResponse(savingGoal);
        }

        public async Task DeleteSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, customerId, cancellationToken);

            if (savingGoal is not null)
            {
                db.SavingGoals.Remove(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            //todo: handle not found case
        }

        public async Task<List<SavingGoalResponse>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await db.SavingGoals.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return savingGoals.Select(s => MapSavingGoalResponse(s)).ToList();
        }

        public async Task<SavingGoalResponse> GetSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, customerId, cancellationToken);
            return MapSavingGoalResponse(savingGoal);
        }

        private async Task<SavingGoal> GetSpecificSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == customerId, cancellationToken) ?? new SavingGoal();
        }

        public async Task<SavingGoalResponse> UpdateSavingGoalAsync(Guid id, UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, request.CustomerId, cancellationToken);

            if (savingGoal is not null)
            {
                savingGoal.Name = request.Name;
                savingGoal.TargetAmount = request.TargetAmount;
                savingGoal.TargetDate = request.TargetDate;

                db.SavingGoals.Update(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            return MapSavingGoalResponse(savingGoal);
        }

        public Task<SavingGoalStatusResponse> GetSavingGoalStatusAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = GetSpecificSavingGoalAsync(id, customerId, cancellationToken);

        }

        public SavingGoalResponse MapSavingGoalResponse(SavingGoal? savingGoal)
        {
            if (savingGoal is null) return new SavingGoalResponse();

            return new SavingGoalResponse
            {
                Id = savingGoal.Id,
                Name = savingGoal.Name,
                TargetAmount = savingGoal.TargetAmount,
                StartDate = savingGoal.StartDate,
                TargetDate = savingGoal.TargetDate,
                CustomerId = savingGoal.CustomerId
            };
        }
    }
}
