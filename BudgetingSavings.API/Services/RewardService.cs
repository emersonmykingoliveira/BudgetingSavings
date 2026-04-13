using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class RewardService(ApiDbContext db, IAccountService accountsService, IConfiguration config) : IRewardService
    {
        public async Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var rewards = await db.Rewards.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return rewards.Select(MapRewardResponse).ToList();
        }

        public async Task<RewardResponse> GetRewardAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var reward = await GetActiveRewardAsync(customerId, cancellationToken);
            return MapRewardResponse(reward);
        }

        private async Task<Reward> GetActiveRewardAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return await db.Rewards.FirstOrDefaultAsync(s => s.CustomerId == customerId && !s.Redeemed, cancellationToken) ?? new Reward();
        }

        public async Task<RedeemRewardResponse> RedeemRewardAsync(Guid customerId, CancellationToken cancellationToken)
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var reward = await db.Rewards.FirstOrDefaultAsync(s => s.CustomerId == customerId && !s.Redeemed, cancellationToken);

                if (reward is null)
                    throw new ArgumentException("No rewards available for redemption.");

                await HandleCashbackRewardAsync(reward, cancellationToken);

                var account = await db.Accounts.FirstOrDefaultAsync(s => s.CustomerId == customerId, cancellationToken);

                if(account is null)
                    throw new ArgumentException("Account not found for the customer.");

                await accountsService.UpdateAccountBalanceAsync(account.Id, account.CustomerId, reward.CashBack, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return MapRedeemRewardResponse(reward, account);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private RedeemRewardResponse MapRedeemRewardResponse(Reward reward, Account account)
        {
            return new RedeemRewardResponse
            {
                CashBack = reward.CashBack,
                UpdatedAccountBalance = account.Balance,
                RedeemedPoints = reward.Points,
                AccountId = account.Id,
                CustomerId = account.CustomerId
            };
        }

        private async Task HandleCashbackRewardAsync(Reward reward, CancellationToken cancellationToken)
        {
            if (reward is null) return;

            var cashBackFactor = config.GetValue<decimal>("RewardCashBackFactor");
            reward.CashBack = reward.Points * cashBackFactor;
            reward.RedeemedDate = DateTime.Now;
            reward.Redeemed = true;
            db.Rewards.Update(reward);
            await db.SaveChangesAsync(cancellationToken);
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

        public async Task RewardHandlerAsync(CreateRewardRequest request, CancellationToken cancellationToken)
        {
            var pointsFactor = config.GetValue<decimal>("RewardPointsFactor");

            var points = (int)(request.Amount * pointsFactor);

            var existingReward = await GetActiveRewardAsync(request.CustomerId, cancellationToken);

            if (existingReward is not null)
            {
                if (request.TransactionType == TransactionType.Credit && request.TransactionCategory == TransactionCategory.Savings)
                {
                    existingReward.Points += points;
                }
                else if (request.TransactionType == TransactionType.Debit)
                {
                    existingReward.Points -= points;
                }

                db.Rewards.Update(existingReward);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (request.TransactionType == TransactionType.Credit && request.TransactionCategory == TransactionCategory.Savings)
                {
                    var reward = new Reward
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = request.CustomerId,
                        Points = points,
                        Date = DateTime.Now,
                        Redeemed = false
                    };

                    db.Rewards.Add(reward);
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
