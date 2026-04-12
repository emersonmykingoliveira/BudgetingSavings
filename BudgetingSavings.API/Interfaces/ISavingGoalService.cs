using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalService
    {
        Task<List<SavingGoalResponse>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<SavingGoalResponse> GetSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
        Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
        Task DeleteSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
        Task<SavingGoalResponse> UpdateSavingGoalAsync(Guid id, UpdateSavingGoalRequest request, CancellationToken cancellationToken);
        Task<SavingGoalStatusResponse> GetSavingGoalStatusAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
    }
}
