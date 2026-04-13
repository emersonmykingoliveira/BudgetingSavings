using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Interfaces
{
    public interface IRewardService
    {
        Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<RewardResponse> GetRewardByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<RedeemRewardResponse> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken);
        Task RewardHandlerAsync(CreateRewardRequest request, CancellationToken cancellationToken);
    }
}
