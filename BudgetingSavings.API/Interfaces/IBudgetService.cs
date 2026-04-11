using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IBudgetService
    {
        Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken);
        Task<BudgetResponse> UpdateBudgetAsync(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken);
        Task DeleteBudgetAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
        Task<BudgetResponse> GetBudgetAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
        Task<List<BudgetResponse>> GetBudgetsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<BudgetStatusResponse> GetBudgetStatusAsync(Guid customerId, Guid id, CancellationToken cancellationToken);
    }
}