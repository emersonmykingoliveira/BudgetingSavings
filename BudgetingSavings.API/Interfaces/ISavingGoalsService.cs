using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalsService
    {
        Task<List<SavingGoal>> GetAllSavingGoalsAsync(Guid accountId, CancellationToken cancellationToken);
        Task<SavingGoal> GetSavingGoalAsync(Guid accountId, Guid id, CancellationToken cancellationToken);
        Task<SavingGoal> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
        Task DeleteSavingGoalAsync(Guid accountId, Guid id, CancellationToken cancellationToken);
        Task<SavingGoal> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken);
    }
}
