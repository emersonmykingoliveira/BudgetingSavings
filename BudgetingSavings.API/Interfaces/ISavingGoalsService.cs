using BudgetingSavings.API.Infrastructure.Entities;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalsService
    {
        Task<List<SavingGoal>> GetAllSavingGoalsAsync(Guid accountId, CancellationToken cancellationToken);
    }
}
