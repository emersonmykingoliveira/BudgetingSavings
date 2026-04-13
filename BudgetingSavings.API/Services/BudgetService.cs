using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Enums;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BudgetingSavings.API.Services
{
    public class BudgetService(ApiDbContext db, 
                                IValidator<CreateBudgetRequest> createValidator,
                                IValidator<UpdateBudgetRequest> updateValidator) : IBudgetService
    {
        public async Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var budget = new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                LimitAmount = request.LimitAmount,
                Currency = request.Currency,
                CustomerId = request.CustomerId
            };

            await db.Budgets.AddAsync(budget, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return MapBudgetResponse(budget);
        }

        public async Task DeleteBudgetAsync(Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(id, cancellationToken);

            if (budget is not null)
            {
                db.Budgets.Remove(budget);
                await db.SaveChangesAsync(cancellationToken);
            }

            //todo: handle not found case
        }

        public async Task<BudgetResponse> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(id, cancellationToken);
            return MapBudgetResponse(budget);
        }

        public async Task<Budget> GetSpecificBudgetAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken) ?? new Budget();
        }

        public async Task<List<BudgetResponse>> GetAllBudgetsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var budgets = await db.Budgets.Where(b => b.CustomerId == customerId).ToListAsync(cancellationToken);
            return budgets.Select(b => MapBudgetResponse(b)).ToList();
        }

        public async Task<BudgetStatusResponse> GetBudgetStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(id, cancellationToken);

            var accounts = await db.Accounts.Where(a => a.CustomerId == budget.CustomerId).ToListAsync(cancellationToken);

            var transactions = await FilterDebitTransactionsForBudget(accounts, budget, cancellationToken);

            var spentAmount = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);

            return MapBudgetStatusResponse(budget, spentAmount);
        }

        private async Task<List<Transaction>> FilterDebitTransactionsForBudget(List<Account> accounts, Budget budget, CancellationToken cancellationToken)
        {
            return await db.Transactions.Where(t => accounts.Select(a => a.Id).Contains(t.AccountId) 
                                            && t.TransactionDateTime >= budget.StartTime
                                            && t.TransactionDateTime <= budget.EndTime
                                            && t.TransactionType == TransactionType.Debit).ToListAsync(cancellationToken);
        }

        public async Task<BudgetResponse> UpdateBudgetAsync(UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var budget = await GetSpecificBudgetAsync(request.Id, cancellationToken);

            if (budget is not null)
            {
                budget.StartTime = request.StartTime;
                budget.EndTime = request.EndTime;
                budget.LimitAmount = request.LimitAmount;
                budget.Currency = request.Currency;
                db.Budgets.Update(budget);
                await db.SaveChangesAsync(cancellationToken);
            }

            return MapBudgetResponse(budget);
        }

        private BudgetResponse MapBudgetResponse(Budget? budget)
        {
            if (budget is null) return new BudgetResponse();

            return new BudgetResponse
            {
                Id = budget.Id,
                StartTime = budget.StartTime,
                EndTime = budget.EndTime,
                LimitAmount = budget.LimitAmount,
                Currency = budget.Currency,
                CustomerId = budget.CustomerId
            };
        }

        private BudgetStatusResponse MapBudgetStatusResponse(Budget budget, decimal spentAmount)
        {
            return new BudgetStatusResponse
            {
                Id = budget.Id,
                StartTime = budget.StartTime,
                EndTime = budget.EndTime,
                LimitAmount = budget.LimitAmount,
                Currency = budget.Currency,
                CustomerId = budget.CustomerId,
                SpentAmount = spentAmount,
                RemainingAmount = budget.LimitAmount - spentAmount,
                IsExceeded = spentAmount > budget.LimitAmount
            };
        }
    }
}
