using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalsController(ISavingGoalService service) : ControllerBase
    {
        [HttpGet("customer/{id}")]
        public async Task<IActionResult> GetAllSavingGoals(Guid id, CancellationToken cancellationToken)
        {
            var savingGoals = await service.GetAllSavingGoalsAsync(id, cancellationToken);
            return Ok(savingGoals);
        }

        [HttpGet("{id}/customer/{customerId}")]
        public async Task<IActionResult> GetSavingGoal(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoal = await service.GetSavingGoalAsync(id, customerId, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request)
        {
            var savingGoal = await service.CreateSavingGoalAsync(request, CancellationToken.None);
            return Ok(savingGoal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingGoal(Guid Id, [FromBody] UpdateSavingGoalRequest request)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(Id, request, CancellationToken.None);
            return Ok(savingGoal);
        }

        [HttpDelete("{id}/customer/{customerId}")]
        public async Task<IActionResult> DeleteSavingGoal(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            await service.DeleteSavingGoalAsync(id, customerId, cancellationToken);
            return NoContent();
        }
    }
}
