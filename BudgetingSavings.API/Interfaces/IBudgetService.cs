using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IBudgetService
    {
        Task<Result<BudgetResponse>> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken);
        Task<Result<BudgetResponse>> UpdateBudgetAsync(UpdateBudgetRequest request, CancellationToken cancellationToken);
        Task<Result> DeleteBudgetAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<BudgetResponse>> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Result<BudgetResponse>>> GetAllBudgetsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<Result<BudgetStatusResponse>> GetBudgetStatusAsync(Guid id, CancellationToken cancellationToken);
    }
}