using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface ISavingGoalService
    {
        Task<Result<List<SavingGoalResponse>>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<Result<SavingGoalResponse>> GetSavingGoalByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<SavingGoalResponse>> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken);
        Task<Result> DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<SavingGoalResponse>> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken);
        Task<Result<SavingGoalStatusResponse>> GetSavingGoalStatusAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<SavingSuggestionsResponse>> GetSavingSuggestions(Guid customerId, CancellationToken cancellationToken);
    }
}
