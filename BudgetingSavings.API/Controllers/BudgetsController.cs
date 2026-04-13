using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    public class BudgetsController(IBudgetService service) : ControllerBase
    {
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetBudgets(Guid customerId, CancellationToken cancellationToken)
        {
            var budgets = await service.GetAllBudgetsAsync(customerId, cancellationToken);
            return Ok(budgets);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBudget(Guid id, CancellationToken cancellationToken)
        {
            var budget = await service.GetBudgetByIdAsync(id, cancellationToken);
            return Ok(budget);
        }

        [HttpGet("{id:guid}/status")]
        public async Task<IActionResult> GetBudgetStatus(Guid id, CancellationToken cancellationToken)
        {
            var budgetStatus = await service.GetBudgetStatusAsync(id, cancellationToken);
            return Ok(budgetStatus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            var budget = await service.CreateBudgetAsync(request, cancellationToken);
            return Ok(budget);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            var budget = await service.UpdateBudgetAsync(request, cancellationToken);
            return Ok(budget);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBudget(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteBudgetAsync(id, cancellationToken);
            return NoContent();
        }
    }
}