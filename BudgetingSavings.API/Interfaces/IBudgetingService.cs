using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public interface IBudgetingService
    {
        Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request);
        Task<BudgetResponse> UpdateBudgetAsync(UpdateBudgetRequest request);
        Task<bool> DeleteBudgetAsync(Guid customerId, Guid id);
        Task<BudgetResponse> GetBudgetAsync(Guid customerId, Guid id);
        Task<List<BudgetResponse>> GetBudgetsAsync(Guid customerId);
        Task<Bud>
    }
}
