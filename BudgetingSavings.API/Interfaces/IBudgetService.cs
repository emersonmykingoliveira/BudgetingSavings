using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IBudgetService
    {
        Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken);
        Task<BudgetResponse> UpdateBudgetAsync(Guid id, Guid customerId, UpdateBudgetRequest request, CancellationToken cancellationToken);
        Task DeleteBudgetAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
        Task<BudgetResponse> GetBudgetAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
        Task<List<BudgetResponse>> GetBudgetsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<BudgetStatusResponse> GetBudgetStatusAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
    }
}