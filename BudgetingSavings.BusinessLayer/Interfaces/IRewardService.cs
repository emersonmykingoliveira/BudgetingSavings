using BudgetingSavings.BusinessLayer.Models.Enums;
using BudgetingSavings.BusinessLayer.Models.Requests;
using BudgetingSavings.BusinessLayer.Models.Responses;

namespace BudgetingSavings.BusinessLayer.Interfaces
{
    public interface IRewardService
    {
        Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<RewardResponse> GetRewardByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<RedeemRewardResponse> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken);
        Task RewardHandlerAsync(CreateRewardRequest request, CancellationToken cancellationToken);
    }
}
