using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalService
    {
        Task<List<SavingGoal>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<SavingGoal> GetSavingGoalAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
        Task<SavingGoal> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
        Task DeleteSavingGoalAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
        Task<SavingGoal> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken);
    }
}
