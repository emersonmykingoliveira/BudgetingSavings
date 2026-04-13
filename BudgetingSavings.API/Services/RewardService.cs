using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Services
{
    public class RewardService(ApiDbContext db) : IRewardService
    {
        public Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RewardResponse> GetRewardAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RewardResponse> RedeemRewardAsync(Guid customerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
