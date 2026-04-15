using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
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
        public async Task<Result<SavingGoalResponse>> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result<SavingGoalResponse>.Fail("Customer does not exist.");

            var activeGoalsCount = await db.SavingGoals
                .CountAsync(g => g.CustomerId == request.CustomerId && g.TargetDate >= DateTime.UtcNow, cancellationToken);

            if (activeGoalsCount >= 5)
                return Result<SavingGoalResponse>.Fail("Customer cannot have more than 5 active saving goals.");

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

            return Result<SavingGoalResponse>.Success(MapSavingGoalResponse(savingGoal));
        }

        public async Task<Result> DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, cancellationToken);

            if (savingGoal is null)
                return Result.Fail("Saving goal not found.");

            db.SavingGoals.Remove(savingGoal);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<List<SavingGoalResponse>>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!customerExists)
                return Result<List<SavingGoalResponse>>.Fail("Customer does not exist.");

            var savingGoals = await db.SavingGoals.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return Result<List<SavingGoalResponse>>.Success(savingGoals.Select(MapSavingGoalResponse).ToList());
        }

        public async Task<Result<SavingGoalResponse>> GetSavingGoalByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, cancellationToken);
            
            if (savingGoal is null)
                return Result<SavingGoalResponse>.Fail("Saving goal not found.");

            return Result<SavingGoalResponse>.Success(MapSavingGoalResponse(savingGoal));
        }

        private async Task<SavingGoal?> GetSpecificSavingGoalAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<Result<SavingGoalResponse>> UpdateSavingGoalAsync(UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var savingGoal = await GetSpecificSavingGoalAsync(request.Id, cancellationToken);

            if (savingGoal is null)
                return Result<SavingGoalResponse>.Fail("Saving goal not found.");

            var amountSaved = await GetAmountSavedFromTransactions(savingGoal, cancellationToken);

            if (request.TargetAmount < amountSaved)
                return Result<SavingGoalResponse>.Fail("Target amount cannot be lower than the amount already saved.");

            savingGoal.Name = request.Name;
            savingGoal.TargetAmount = request.TargetAmount;
            savingGoal.TargetDate = request.TargetDate;

            db.SavingGoals.Update(savingGoal);
            await db.SaveChangesAsync(cancellationToken);

            return Result<SavingGoalResponse>.Success(MapSavingGoalResponse(savingGoal));
        }

        public async Task<Result<SavingGoalStatusResponse>> GetSavingGoalStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, cancellationToken);

            if (savingGoal is null)
                return Result<SavingGoalStatusResponse>.Fail("Saving goal not found.");

            var amountSaved = await GetAmountSavedFromTransactions(savingGoal, cancellationToken);

            return Result<SavingGoalStatusResponse>.Success(MapSavingGoalStatusResponse(savingGoal, amountSaved));
        }

        private async Task<decimal> GetAmountSavedFromTransactions(SavingGoal savingGoal, CancellationToken cancellationToken)
        {
            return await db.Transactions
                        .Where(t => t.CustomerId == savingGoal.CustomerId
                        && t.TransactionDateTime >= savingGoal.StartDate
                        && t.TransactionDateTime <= savingGoal.TargetDate
                        && t.TransactionCategory == TransactionCategory.Savings)
                        .SumAsync(t => t.Amount, cancellationToken);
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

        private SavingGoalStatusResponse MapSavingGoalStatusResponse(SavingGoal savingGoal, decimal amountSaved)
        {
            return new SavingGoalStatusResponse
            {
                Id = savingGoal.Id,
                Name = savingGoal.Name,
                SavedAmount = amountSaved,
                TargetAmount = savingGoal.TargetAmount,
                CustomerId= savingGoal.CustomerId,
                RemainingAmount = savingGoal.TargetAmount - amountSaved,
                ProgressPercentage = savingGoal.TargetAmount > 0 ? (amountSaved / savingGoal.TargetAmount) * 100 : 0,
                StartDate = savingGoal.StartDate,
                TargetDate = savingGoal.TargetDate,
                Status = DefineSavingGoalStatus(savingGoal, amountSaved),
                DaysRemaining = Math.Max(0, (savingGoal.TargetDate - DateTime.UtcNow).Days)
            };
        }

        private SavingGoalStatus DefineSavingGoalStatus(SavingGoal savingGoal, decimal savedAmount)
        {
            return savingGoal.TargetDate < DateTime.UtcNow
                ? (savedAmount >= savingGoal.TargetAmount ? SavingGoalStatus.Completed : SavingGoalStatus.Failed)
                : (savedAmount > 0 ? SavingGoalStatus.InProgress : SavingGoalStatus.NotStarted);
        }

        public async Task<Result<SavingSuggestionsResponse>> GetSavingSuggestions(Guid customerId, CancellationToken cancellationToken)
        {
            var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!customerExists)
                return Result<SavingSuggestionsResponse>.Fail("Customer does not exist.");

            var hasTransactions = await db.Transactions.AnyAsync(t => t.CustomerId == customerId, cancellationToken);

            if (!hasTransactions)
                return Result<SavingSuggestionsResponse>.Fail("Not enough transaction data to generate saving suggestions.");

            var income = await db.Transactions
                            .Where(t => t.CustomerId == customerId && t.TransactionType == TransactionType.Credit)
                            .SumAsync(t => t.Amount, cancellationToken);

            var expenses = await db.Transactions
                            .Where(t => t.CustomerId == customerId && t.TransactionType == TransactionType.Debit)
                            .SumAsync(t => Math.Abs(t.Amount), cancellationToken);

            var disposable = income - expenses;

            if (disposable <= 0)
            {
                return Result<SavingSuggestionsResponse>.Success(new SavingSuggestionsResponse
                {
                    CustomerId = customerId,
                    Income = income,
                    Expenses = expenses,
                    Disposable = disposable
                });
            }

            return Result<SavingSuggestionsResponse>.Success(CalculateSavingSuggestions(customerId, income, expenses, disposable));
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
