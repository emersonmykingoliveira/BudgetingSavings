using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalService
    {
        Task<List<SavingGoalResponse>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<SavingGoalResponse> GetSavingGoalByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
        Task DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken);
        Task<SavingGoalResponse> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken);
        Task<SavingGoalStatusResponse> GetSavingGoalStatusAsync(Guid id, CancellationToken cancellationToken);
        Task<SavingSuggestionsResponse> GetSavingSuggestions(Guid customerId, CancellationToken cancellationToken);
    }
}
