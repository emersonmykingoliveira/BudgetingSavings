using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BudgetingSavings.API.Services
{
    public class RewardService(ApiDbContext db, 
                               IAccountService accountsService, 
                               IConfiguration config,
                               IValidator<CreateRewardRequest> createValidator) : IRewardService
    {
        public async Task<List<RewardResponse>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var rewards = await db.Rewards.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return rewards.Select(MapRewardResponse).ToList();
        }

        public async Task<RewardResponse> GetRewardByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var reward = await GetActiveRewardAsync(id, cancellationToken);
            return MapRewardResponse(reward);
        }

        private async Task<Reward?> GetActiveRewardAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Rewards.FirstOrDefaultAsync(s => s.Id == id && !s.Redeemed, cancellationToken);
        }

        public async Task<RedeemRewardResponse> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var reward = await db.Rewards.FirstOrDefaultAsync(s => s.Id == request.Id 
                                                                    && s.CustomerId == request.CustomerId
                                                                    && !s.Redeemed, cancellationToken);

                if (reward is null)
                    throw new ArgumentException("No rewards available for redemption.");

                await HandleCashbackRewardAsync(reward, cancellationToken);

                var account = await db.Accounts.FirstOrDefaultAsync(s => s.CustomerId == request.CustomerId, cancellationToken);

                if(account is null)
                    throw new ArgumentException("Account not found for the customer.");

                await accountsService.UpdateAccountBalanceAsync(account.Id, reward.CashBack, cancellationToken);

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
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var pointsFactor = config.GetValue<decimal>("RewardPointsFactor");
            var points = (int)(request.Amount * pointsFactor);

            if (points == 0) return;

            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var existingReward = await db.Rewards.FirstOrDefaultAsync(s => s.CustomerId == request.CustomerId && !s.Redeemed, cancellationToken);

                if (existingReward is not null)
                {
                    await HandleExistingRewardAsync(existingReward, points, request, cancellationToken);
                }
                else if (request.TransactionType == TransactionType.Credit && request.TransactionCategory == TransactionCategory.Savings)
                {
                    await HandleNewRewardAsync(points, request, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task HandleExistingRewardAsync(Reward existingReward, int points, CreateRewardRequest request, CancellationToken cancellationToken)
        {
            var originalPoints = existingReward.Points;

            if (request.TransactionType == TransactionType.Credit && request.TransactionCategory == TransactionCategory.Savings)
            {
                if (await IsFirstSavingOfMonthAsync(request.CustomerId, cancellationToken))
                {
                    points += 50;
                }
                existingReward.Points += points;
            }
            else if (request.TransactionType == TransactionType.Debit)
            {
                existingReward.Points = Math.Max(0, existingReward.Points - points);
            }

            if (existingReward.Points != originalPoints)
            {
                db.Rewards.Update(existingReward);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task HandleNewRewardAsync(int points, CreateRewardRequest request, CancellationToken cancellationToken)
        {
            var newReward = new Reward
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Points = points + 100,
                Date = DateTime.Now,
                Redeemed = false
            };

            db.Rewards.Add(newReward);
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task<bool> IsFirstSavingOfMonthAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken);
            if (account == null) return false;

            return !await db.Transactions.AnyAsync(t =>
                t.AccountId == account.Id &&
                t.TransactionDateTime.Month == DateTime.Now.Month &&
                t.TransactionDateTime.Year == DateTime.Now.Year &&
                t.TransactionCategory == TransactionCategory.Savings, cancellationToken);
        }
    }
}
