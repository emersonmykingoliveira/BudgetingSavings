using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class RewardService(ApiDbContext db, IAccountService accountsService) : IRewardService
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

        public async Task<RedeemRewardResponse> RedeemRewardAsync(Guid customerId, CancellationToken cancellationToken)
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var rewards = await db.Rewards
                    .Where(s => s.CustomerId == customerId && !s.Redeemed)
                    .ToListAsync(cancellationToken);

                decimal cashBackTotal = await CashbackRewardsAsync(rewards, cancellationToken);

                var account = await db.Accounts
                    .FirstOrDefaultAsync(s => s.CustomerId == customerId, cancellationToken);

                await accountsService.UpdateAccountBalanceAsync(account?.Id ?? Guid.Empty, account?.CustomerId ?? Guid.Empty, cashBackTotal, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return MapRedeemRewardResponse(rewards, account);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private RedeemRewardResponse MapRedeemRewardResponse(List<Reward> rewards, Account? account)
        {
            return new RedeemRewardResponse
            {
                CashBack = rewards.Sum(s => s.CashBack),
                UpdatedAccountBalance = account?.Balance ?? 0m,
                RedeemedPoints = rewards.Sum(s => s.Points),
                AccountId = account?.Id ?? Guid.Empty,
                CustomerId = account?.CustomerId ?? Guid.Empty 
            };
        }

        private async Task<decimal> CashbackRewardsAsync(List<Reward> rewards, CancellationToken cancellationToken)
        {
            foreach (var reward in rewards)
            {
                reward.CashBack = reward.Points * 0.01m;
                reward.RedeemedDate = DateTime.Now;
                reward.Redeemed = true;
                db.Rewards.Update(reward);
                await db.SaveChangesAsync(cancellationToken);
            }

            return rewards.Sum(s => s.CashBack);
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
