using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace BudgetingSavings.API.Services
{
    public class RewardService(ApiDbContext db,
                                IValidator<CreateRewardRequest> createValidator,
                                IConfiguration config) : IRewardService
    {
        public async Task<Result<List<RewardResponse>>> GetAllRewardsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customerExists = await db.Customers
                .AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!customerExists)
                return Result<List<RewardResponse>>.Fail("Customer does not exist.");

            var rewards = await db.Rewards
                .Where(r => r.CustomerId == customerId)
                .ToListAsync(cancellationToken);

            return Result<List<RewardResponse>>.Success(rewards.Select(MapRewardResponse).ToList());
        }

        public async Task<Result<RewardResponse>> GetRewardByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var reward = await GetActiveRewardAsync(id, cancellationToken);
            
            if (reward is null)
                return Result<RewardResponse>.Fail("Reward does not exist or has already been redeemed.");

            return Result<RewardResponse>.Success(MapRewardResponse(reward));
        }

        private async Task<Reward?> GetActiveRewardAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Rewards.FirstOrDefaultAsync(r => r.Id == id && !r.Redeemed, cancellationToken);
        }

        public async Task<Result<RedeemRewardResponse>> RedeemRewardAsync(RedeemRewardRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var account = await db.Accounts.FirstOrDefaultAsync(a => a.CustomerId == request.CustomerId, cancellationToken);

                if (account is null)
                    return Result<RedeemRewardResponse>.Fail("Account not found for the customer.");

                var reward = await db.Rewards.FirstOrDefaultAsync(r => r.Id == request.Id &&
                                                                r.CustomerId == request.CustomerId &&
                                                                !r.Redeemed && r.Points > 0,
                                                                cancellationToken);
                if (reward is null)
                    return Result<RedeemRewardResponse>.Fail("No rewards available for redemption.");

                var cashbackResult = await HandleCashbackRewardAsync(reward, cancellationToken);

                if (cashbackResult.IsFailure)
                    return Result<RedeemRewardResponse>.Fail(cashbackResult.Error ?? "An error occurred while processing the cashback reward.");

                await AddCashBackToAccountBalance(account, reward);
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<RedeemRewardResponse>.Success(MapRedeemRewardResponse(reward, account));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<RedeemRewardResponse>.Fail(ex.Message);
            }
        }

        private async Task AddCashBackToAccountBalance(Account account, Reward reward)
        {
            account.Balance += reward.CashBack;
            account.LastTransactionDate = DateTime.UtcNow;
            db.Accounts.Update(account);
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

        private async Task<Result> HandleCashbackRewardAsync(Reward reward, CancellationToken cancellationToken)
        {
            var cashBackFactor = config.GetValue<decimal>("RewardSettings:CashBackFactor");
            if (cashBackFactor <= 0)
                return Result.Fail("Reward cashback factor is invalid.");

            var cashback = reward.Points * (cashBackFactor / 100);
            if (cashback <= 0)
                return Result.Fail("Reward cashback amount is invalid.");

            reward.CashBack = cashback;
            reward.RedeemedDate = DateTime.UtcNow;
            reward.Redeemed = true;

            db.Rewards.Update(reward);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        private RewardResponse MapRewardResponse(Reward reward)
        {
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

        public async Task<Result> RewardHandlerAsync(CreateRewardRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);
            if (!customerExists)
                return Result.Fail("Customer does not exist.");

            var pointsResult = await CalculatePoints(request.Amount);

            if (pointsResult.IsFailure) 
                return Result.Fail(pointsResult.Error ?? "An error occurred while calculating points.");

            var points = pointsResult.Value;

            return await RewardTransactionHandlerAsync(request, points, cancellationToken);
        }

        private async Task<Result> RewardTransactionHandlerAsync(CreateRewardRequest request, int points, CancellationToken cancellationToken)
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var existingReward = await db.Rewards
                    .FirstOrDefaultAsync(r => r.CustomerId == request.CustomerId && !r.Redeemed, cancellationToken);

                var isFirstTransaction = !await db.Rewards.AnyAsync(r => r.CustomerId == request.CustomerId, cancellationToken);

                if (existingReward is not null)
                {
                    if (IsSavingsTransaction(request))
                    {
                        await HandleExistingRewardAsync(existingReward, points, request, cancellationToken);
                    }
                }
                else if (isFirstTransaction)
                {
                    int initialPoints = (request.TransactionType == TransactionType.Credit && request.TransactionCategory == TransactionCategory.Savings) ? points : 0;
                    await HandleNewRewardAsync(initialPoints, request, cancellationToken);
                }
                else if (request.TransactionType == TransactionType.Credit && request.TransactionCategory == TransactionCategory.Savings)
                {
                    await HandleNewRewardAsync(points, request, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Fail(ex.Message);
            }
        }

        private Task<Result<int>> CalculatePoints(decimal amount)
        {
            var pointsFactor = config.GetValue<decimal>("RewardSettings:PointsFactor");
            if (pointsFactor <= 0)
                return Task.FromResult(Result<int>.Fail("Reward points factor is invalid."));

            var points = (int)Math.Round(amount * (pointsFactor / 100), MidpointRounding.AwayFromZero);

            return Task.FromResult(Result<int>.Success(points));
        }

        private bool IsSavingsTransaction(CreateRewardRequest request)
        {
            bool isSavingsCredit = request.TransactionType == TransactionType.Credit &&
                                   request.TransactionCategory == TransactionCategory.Savings;

            bool isSavingsDebit = request.TransactionType == TransactionType.Debit &&
                                  request.TransactionCategory == TransactionCategory.Savings;

            return isSavingsCredit || isSavingsDebit;
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
                Date = DateTime.UtcNow,
                Redeemed = false
            };

            db.Rewards.Add(newReward);
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task<bool> IsFirstSavingOfMonthAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var otherSavingsCount = await db.Transactions.CountAsync(t =>
                t.CustomerId == customerId &&
                t.TransactionDateTime >= startOfMonth &&
                t.TransactionCategory == TransactionCategory.Savings,
                cancellationToken);

            return otherSavingsCount == 1;
        }
    }
}