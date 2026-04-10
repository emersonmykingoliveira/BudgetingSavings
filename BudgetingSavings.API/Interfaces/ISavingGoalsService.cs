using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalsService
    {
        Task<List<SavingGoal>> GetAllSavingGoalsAsync(CancellationToken cancellationToken);
        Task<SavingGoal> GetSavingGoalAsync(Guid id, CancellationToken cancellationToken);
        Task<SavingGoal> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
        Task DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken);
        Task<SavingGoal> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken);
    }
}
