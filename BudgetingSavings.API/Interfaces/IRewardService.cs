using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Interfaces
{
    public interface IRewardService
    {
        Task<List<Result<RewardResponse>>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken);
        Task<Result<RewardResponse>> GetRewardByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<RedeemRewardResponse>> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken);
        Task<Result> RewardHandlerAsync(CreateRewardRequest request, CancellationToken cancellationToken);
    }
}
