using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class RewardService(ApiDbContext db) : IRewardService
    {
        public async Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var rewards = await db.Rewards.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return rewards.Select(MapRewardResponse).ToList();
        }

        public async Task<RewardResponse> GetRewardAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var reward = await db.Rewards.FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == customerId, cancellationToken);
            return MapRewardResponse(reward);
        }

        public Task<RewardResponse> RedeemRewardAsync(Guid customerId, CancellationToken cancellationToken)
        {
            //Get all rewards that are not redeemed
            var rewards = db.Rewards.Where(s => s.CustomerId == customerId && !s.Redeemed).ToList();

            decimal cashBackTotal = 0;

            foreach (var reward in rewards)
            {
                reward.CashBack = reward.Points * 0.01m;
                db.Rewards.Update(reward);
                db.SaveChangesAsync(cancellationToken);
                reward.RedeemedDate = DateTime.Now;
                reward.Redeemed = true;
                cashBackTotal += reward.CashBack;
            }
            //Calculate cash back and points


            //Update account balance with cash back

        }

        private RewardResponse MapRewardResponse(Reward? reward)
        {
            if (reward is null) return new RewardResponse();
            return new RewardResponse
            {
                Id = reward.Id,
                CashBack = reward.CashBack,
                Date = reward.Date,
                Points = reward.Points,
                Redeemed = reward.Redeemed,
                RedeemedDate = reward.RedeemedDate,
                CustomerId = reward.CustomerId
            };
        }
    }
}
