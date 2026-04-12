using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace BudgetingSavings.API.Services
{
    public class SavingGoalService(ApiDbContext db) : ISavingGoalService
    {
        public async Task<SavingGoalResponse> CreateSavingGoalAsync(CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                TargetAmount = request.TargetAmount,
                StartDate = DateTime.Now,
                TargetDate = request.TargetDate,
                CustomerId = request.CustomerId
            };

            await db.SavingGoals.AddAsync(savingGoal, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return MapSavingGoalResponse(savingGoal);
        }

        public async Task DeleteSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, customerId, cancellationToken);

            if (savingGoal is not null)
            {
                db.SavingGoals.Remove(savingGoal);
                await db.SaveChangesAsync(cancellationToken);
            }

            //todo: handle not found case
        }

        public async Task<List<SavingGoalResponse>> GetAllSavingGoalsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await db.SavingGoals.Where(s => s.CustomerId == customerId).ToListAsync(cancellationToken);
            return savingGoals.Select(s => MapSavingGoalResponse(s)).ToList();
        }

        public async Task<SavingGoalResponse> GetSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, customerId, cancellationToken);
            return MapSavingGoalResponse(savingGoal);
        }

        private async Task<SavingGoal> GetSpecificSavingGoalAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            return await db.SavingGoals.FirstOrDefaultAsync(s => s.Id == id && s.CustomerId == customerId, cancellationToken) ?? new SavingGoal();
        }

        public async Task<SavingGoalResponse> UpdateSavingGoalAsync(Guid id, UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, request.CustomerId, cancellationToken);

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

        public async Task<SavingGoalStatusResponse> GetSavingGoalStatusAsync(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = await GetSpecificSavingGoalAsync(id, customerId, cancellationToken);

            var accounts = await db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync(cancellationToken);

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

        public SavingGoalResponse MapSavingGoalResponse(SavingGoal? savingGoal)
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
                DaysRemaining = (savingGoal.TargetDate - DateTime.Now).Days
            };
        }

        private SavingGoalStatus DefineSavingGoalStatus(SavingGoal savingGoal, decimal savedAmount)
        {
            return savingGoal.TargetDate < DateTime.Now
                ? (savedAmount >= savingGoal.TargetAmount ? SavingGoalStatus.Completed : SavingGoalStatus.Failed)
                : (savedAmount > 0 ? SavingGoalStatus.InProgress : SavingGoalStatus.NotStarted);
        }
    }
}
