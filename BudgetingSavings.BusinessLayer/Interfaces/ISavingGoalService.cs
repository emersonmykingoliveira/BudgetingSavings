using BudgetingSavings.BusinessLayer.Infrastructure.Entities;
using BudgetingSavings.BusinessLayer.Models.Requests;
using BudgetingSavings.BusinessLayer.Models.Responses;

namespace BudgetingSavings.BusinessLayer.Services
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
