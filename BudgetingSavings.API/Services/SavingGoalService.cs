using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace BudgetingSavings.API.Services
{
    public class SavingGoalService(ApiDbContext db, 
                                    IValidator<CreateSavingGoalRequest> createValidator,
                                    IValidator<UpdateSavingGoalRequest> updateValidator) : ISavingGoalService
    {
        public async Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var savingGoal = new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                TargetAmount = request.TargetAmount,
                StartDate = DateTime.UtcNow,
                TargetDate = request.TargetDate,
                CustomerId = request.CustomerId
            };

            await db.SavingGoals.AddAsync(savingGoal, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return MapSavingGoalResponse(savingGoal);
        }

        public async Task DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, cancellationToken);

            if (savingGoal is not null)
            {
                db.SavingGoals.Remove(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<SavingGoalResponse>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await db.SavingGoals.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return savingGoals.Select(s => MapSavingGoalResponse(s)).ToList();
        }

        public async Task<SavingGoalResponse> GetSavingGoalByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, cancellationToken);
            return MapSavingGoalResponse(savingGoal);
        }

        private async Task<SavingGoal> GetSpecificSavingGoalAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new SavingGoal();
        }

        public async Task<SavingGoalResponse> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var savingGoal = await GetSpecificSavingGoalAsync(request.Id, cancellationToken);

            if (savingGoal is not null)
            {
                savingGoal.Name = request.Name;
                savingGoal.TargetAmount = request.TargetAmount;
                savingGoal.TargetDate = request.TargetDate;

                db.SavingGoals.Update(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            return MapSavingGoalResponse(savingGoal);
        }

        public async Task<SavingGoalStatusResponse> GetSavingGoalStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, cancellationToken);

            var accounts = await db.Accounts.Where(a => a.CustomerId == savingGoal.CustomerId).ToListAsync(cancellationToken);

            var transactions = await FilterCreditTransactionsForSaving(savingGoal, accounts, cancellationToken);

            return MapSavingGoalStatusResponse(savingGoal, transactions);
        }

        private async Task<List<Transaction>> FilterCreditTransactionsForSaving(SavingGoal savingGoal, List<Account> accounts, CancellationToken cancellationToken)
        {
            return await db.Transactions
                        .Where(t => accounts.Select(a => a.Id).Contains(t.AccountId)
                        && t.TransactionDateTime >= savingGoal.StartDate
                        && t.TransactionDateTime <= savingGoal.TargetDate
                        && t.TransactionType == TransactionType.Credit
                        && t.TransactionCategory == TransactionCategory.Savings).ToListAsync(cancellationToken);
        }

        private SavingGoalResponse MapSavingGoalResponse(SavingGoal? savingGoal)
        {
            if (savingGoal is null) return new SavingGoalResponse();

            return new SavingGoalResponse
            {
                Id = savingGoal.Id,
                Name = savingGoal.Name,
                TargetAmount = savingGoal.TargetAmount,
                StartDate = savingGoal.StartDate,
                TargetDate = savingGoal.TargetDate,
                CustomerId = savingGoal.CustomerId
            };
        }

        private SavingGoalStatusResponse MapSavingGoalStatusResponse(SavingGoal savingGoal, List<Transaction> transactions)
        {
            var savedAmount = transactions.Sum(t => t.Amount);

            return new SavingGoalStatusResponse
            {
                Id = savingGoal.Id,
                Name = savingGoal.Name,
                SavedAmount = savedAmount,
                TargetAmount = savingGoal.TargetAmount,
                CustomerId= savingGoal.CustomerId,
                RemainingAmount = savingGoal.TargetAmount - savedAmount,
                ProgressPercentage = savingGoal.TargetAmount > 0 ? (savedAmount / savingGoal.TargetAmount) * 100 : 0,
                StartDate = savingGoal.StartDate,
                TargetDate = savingGoal.TargetDate,
                Status = DefineSavingGoalStatus(savingGoal, savedAmount),
                DaysRemaining = (savingGoal.TargetDate - DateTime.UtcNow).Days
            };
        }

        private SavingGoalStatus DefineSavingGoalStatus(SavingGoal savingGoal, decimal savedAmount)
        {
            return savingGoal.TargetDate < DateTime.UtcNow
                ? (savedAmount >= savingGoal.TargetAmount ? SavingGoalStatus.Completed : SavingGoalStatus.Failed)
                : (savedAmount > 0 ? SavingGoalStatus.InProgress : SavingGoalStatus.NotStarted);
        }

        public async Task<SavingSuggestionsResponse> GetSavingSuggestions(Guid customerId, CancellationToken cancellationToken)
        {
            var income = await db.Transactions
                .Where(t => t.CustomerId == customerId && t.TransactionType == TransactionType.Credit)
                .SumAsync(t => t.Amount, cancellationToken);

            var expenses = await db.Transactions
                .Where(t => t.CustomerId == customerId && t.TransactionType == TransactionType.Debit)
                .SumAsync(t => t.Amount, cancellationToken);

            var disposable = income - expenses;

            if (disposable <= 0)
            {
                return new SavingSuggestionsResponse
                {
                    CustomerId = customerId,
                    Income = income,
                    Expenses = expenses,
                    Disposable = disposable
                };
            }

            return CalculateSavingSuggestions(customerId, income, expenses, disposable);
        }

        private SavingSuggestionsResponse CalculateSavingSuggestions(Guid customerId, decimal income, decimal expenses, decimal disposable)
        {
            var recommendedPercentage = CalculateRecommendedPercentage(income, expenses, disposable);
            var recommendedMontlySaving = CalculateRecommendedMontlySaving(recommendedPercentage, disposable);
            var suggestedTargetAmount = CalculateSuggestedTargetAmount(income, expenses, disposable);
            var estimatedMonths = CalculateEstimatedMonths(recommendedMontlySaving, suggestedTargetAmount);

            return new SavingSuggestionsResponse
            {
                CustomerId = customerId,
                Income = income,
                Expenses = expenses,
                Disposable = disposable,
                SavingPercentage = recommendedPercentage,
                RecommendedMontlySaving = recommendedMontlySaving,
                SuggestedTargetAmount = suggestedTargetAmount,
                ExtimatedMonths = estimatedMonths
            };
        }

        private decimal CalculateRecommendedMontlySaving(decimal recommendedPercentage, decimal disposable)
        {
            var recommendedMonthlySaving = disposable * recommendedPercentage;
            return Math.Max(0, Math.Round(recommendedMonthlySaving, 2));
        }

        private decimal CalculateRecommendedPercentage(decimal income, decimal expenses, decimal disposable)
        {
            var savingsCapacityRatio = income <= 0 ? 0 : disposable / income;

            decimal savingPercentage = savingsCapacityRatio switch
            {
                <= 0.05m => 0.00m,
                <= 0.10m => 0.05m,
                <= 0.20m => 0.10m,
                <= 0.30m => 0.15m,
                _ => 0.20m
            };

            return savingPercentage;
        }

        private decimal CalculateSuggestedTargetAmount(decimal income, decimal expenses, decimal disposable)
        {
            var savingsCapacityRatio = income <= 0 ? 0 : disposable / income;

            int targetMonthsOfExpenses = savingsCapacityRatio switch
            {
                <= 0.10m => 1,
                <= 0.20m => 2,
                _ => 3
            };

            return Math.Round(expenses * targetMonthsOfExpenses, 2);
        }

        private int CalculateEstimatedMonths(decimal recommendedMonthlySaving, decimal suggestedTargetAmount)
        {
            if (recommendedMonthlySaving <= 0 || suggestedTargetAmount <= 0)
                return 0;

            var estimatedMonths = (int)Math.Ceiling(suggestedTargetAmount / recommendedMonthlySaving);

            if (estimatedMonths > 24)
                return 12;

            return estimatedMonths;
        }
    }
}
