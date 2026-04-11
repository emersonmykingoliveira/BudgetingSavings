using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController(IBudgetService service) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(Guid id, CancellationToken cancellationToken)
        {
            var budget = await service.GetBudgetsAsync(id, cancellationToken);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingGoal(Guid id, [FromBody] UpdateBudgetRequest request)
        {
            var savingGoal = await service.UpdateBudgetAsync(id, request, CancellationToken.None);
            return Ok(savingGoal);
        }
    }
}
