using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IBudgetService
    {
        Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken);
        Task<BudgetResponse> UpdateBudgetAsync(UpdateBudgetRequest request, CancellationToken cancellationToken);
        Task DeleteBudgetAsync(Guid id, CancellationToken cancellationToken);
        Task<BudgetResponse> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<BudgetResponse>> GetAllBudgetsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<BudgetStatusResponse> GetBudgetStatusAsync(Guid id, CancellationToken cancellationToken);
    }
}