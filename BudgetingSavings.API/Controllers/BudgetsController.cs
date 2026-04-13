using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController(IBudgetService service) : ControllerBase
    {
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetBudgets(Guid customerId, CancellationToken cancellationToken)
        {
            var budget = await service.GetAllBudgetsAsync(customerId, cancellationToken);
            return Ok(budget);
        }

        [HttpGet("{id}/customer/{customerId}")]
        public async Task<IActionResult> GetBudget(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var budget = await service.GetBudgetAsync(id, customerId, cancellationToken);
            return Ok(budget);
        }

        [HttpGet("{id}/customer/{customerId}/Status")]
        public async Task<IActionResult> GetBudgetStatus(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var budget = await service.GetBudgetStatusAsync(id, customerId, cancellationToken);
            return Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            var budget = await service.CreateBudgetAsync(request, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id}/customer/{customerId}")]
        public async Task<IActionResult> DeleteBudget(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            await service.DeleteBudgetAsync(id, customerId, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id}/customer/{customerId}")]
        public async Task<IActionResult> UpdateBudget(Guid id, Guid customerId, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            var budget = await service.UpdateBudgetAsync(id, customerId, request, cancellationToken);
            return Ok(budget);
        }
    }
}
