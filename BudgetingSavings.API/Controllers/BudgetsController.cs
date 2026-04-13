using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:guid}/budgets")]
    public class BudgetsController(IBudgetService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetBudgets(Guid customerId, CancellationToken cancellationToken)
        {
            var budgets = await service.GetAllBudgetsAsync(customerId, cancellationToken);
            return Ok(budgets);
        }

        [HttpGet("{budgetId:guid}")]
        public async Task<IActionResult> GetBudget(Guid customerId, Guid budgetId, CancellationToken cancellationToken)
        {
            var budget = await service.GetBudgetAsync(budgetId, customerId, cancellationToken);
            return Ok(budget);
        }

        [HttpGet("{budgetId:guid}/status")]
        public async Task<IActionResult> GetBudgetStatus(Guid customerId, Guid budgetId, CancellationToken cancellationToken)
        {
            var budgetStatus = await service.GetBudgetStatusAsync(budgetId, customerId, cancellationToken);
            return Ok(budgetStatus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget(Guid customerId, [FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            request.CustomerId = customerId;
            var budget = await service.CreateBudgetAsync(request, cancellationToken);
            return Ok(budget);
        }

        [HttpPut("{budgetId:guid}")]
        public async Task<IActionResult> UpdateBudget(Guid customerId, Guid budgetId, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            var budget = await service.UpdateBudgetAsync(budgetId, customerId, request, cancellationToken);
            return Ok(budget);
        }

        [HttpDelete("{budgetId:guid}")]
        public async Task<IActionResult> DeleteBudget(Guid customerId, Guid budgetId, CancellationToken cancellationToken)
        {
            await service.DeleteBudgetAsync(budgetId, customerId, cancellationToken);
            return NoContent();
        }
    }
}