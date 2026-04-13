using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Interfaces
{
    public interface IRewardService
    {
        Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<RewardResponse> GetRewardAsync(Guid id, Guid customerId, CancellationToken cancellationToken);
        Task<RewardResponse> RedeemRewardAsync(Guid customerId, CancellationToken cancellationToken);
    }
}
