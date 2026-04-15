using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BudgetingSavings.API.Services
{
    public class BudgetService(ApiDbContext db, 
                                IValidator<CreateBudgetRequest> createValidator,
                                IValidator<UpdateBudgetRequest> updateValidator) : IBudgetService
    {
        public async Task<Result<BudgetResponse>> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (!customerExists)
                return Result<BudgetResponse>.Fail("Customer does not exist.");

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
            return Result<BudgetResponse>.Success(MapBudgetResponse(budget));
        }

        public async Task<Result> DeleteBudgetAsync(Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(id, cancellationToken);

            if (budget is null)
                return Result.Fail("Budget does not exist.");

            db.Budgets.Remove(budget);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<BudgetResponse>> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(id, cancellationToken);

            if (budget is null)
                return Result<BudgetResponse>.Fail("Budget does not exist.");

            return Result<BudgetResponse>.Success(MapBudgetResponse(budget));
        }

        private async Task<Budget?> GetSpecificBudgetAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Budgets.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Result<List<BudgetResponse>>> GetAllBudgetsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customerExists = await db.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!customerExists)
                return Result<List<BudgetResponse>>.Fail("Customer does not exist.");

            var budgets = await db.Budgets.Where(b => b.CustomerId == customerId).ToListAsync(cancellationToken);
            return Result<List<BudgetResponse>>.Success(budgets.Select(MapBudgetResponse).ToList());
        }

        public async Task<Result<BudgetStatusResponse>> GetBudgetStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(id, cancellationToken);

            if (budget is null)
                return Result<BudgetStatusResponse>.Fail("Budget does not exist.");

            var spentAmount = await db.Transactions
                .Where(t => t.CustomerId == budget.CustomerId
                        && t.TransactionDateTime >= budget.StartTime
                        && t.TransactionDateTime <= budget.EndTime
                        && t.TransactionType == TransactionType.Debit)
                .SumAsync(t => Math.Abs(t.Amount), cancellationToken);

            return Result<BudgetStatusResponse>.Success(MapBudgetStatusResponse(budget, spentAmount));
        }

        public async Task<Result<BudgetResponse>> UpdateBudgetAsync(UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var budget = await GetSpecificBudgetAsync(request.Id, cancellationToken);

            if (budget is null)
                return Result<BudgetResponse>.Fail("Budget does not exist.");

            budget.StartTime = request.StartTime;
            budget.EndTime = request.EndTime;
            budget.LimitAmount = request.LimitAmount;
            budget.Currency = request.Currency;
            db.Budgets.Update(budget);
            await db.SaveChangesAsync(cancellationToken);

            return Result<BudgetResponse>.Success(MapBudgetResponse(budget));
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
