using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class BudgetingService(ApiDbContext db) : IBudgetingService
    {
        public Task<BudgetResponse> CreateBudgetAsync(CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteBudgetAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(customerId, id, cancellationToken);

            if(budget is not null)
            {
                db.Budgets.Remove(budget);
                await db.SaveChangesAsync(cancellationToken);
            }

            //todo: handle not found case
        }

        public async Task<BudgetResponse> GetBudgetAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(customerId, id, cancellationToken);
            return MapBudgetResponse(budget);
        }

        public async Task<Budget> GetSpecificBudgetAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            return await db.Budgets.FirstOrDefaultAsync(b => b.CustomerId == customerId && b.Id == id, cancellationToken) ?? new Budget();
        }

        public async Task<List<BudgetResponse>> GetBudgetsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var budgets = await db.Budgets.Where(b => b.CustomerId == customerId).ToListAsync();
            return budgets.Select(b => MapBudgetResponse(b)).ToList();
        }

        public Task<BudgetStatusResponse> GetBudgetStatusAsync(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<BudgetResponse> UpdateBudgetAsync(UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            var budget = await GetSpecificBudgetAsync(request.CustomerId, request.Id, cancellationToken);

            if(budget is not null)
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
            if(budget is null) return new BudgetResponse();

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
    }
}
