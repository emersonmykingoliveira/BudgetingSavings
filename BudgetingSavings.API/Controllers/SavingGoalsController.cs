using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/saving-goals")]
    public class SavingGoalsController(ISavingGoalService service) : ControllerBase
    {
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetAllSavingGoals(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await service.GetAllSavingGoalsAsync(customerId, cancellationToken);
            return Ok(savingGoals);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSavingGoal(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await service.GetSavingGoalByIdAsync(id, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpGet("{id:guid}/status")]
        public async Task<IActionResult> GetSavingGoalStatus(Guid id, CancellationToken cancellationToken)
        {
            var savingGoalStatus = await service.GetSavingGoalStatusAsync(id, cancellationToken);
            return Ok(savingGoalStatus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.CreateSavingGoalAsync(request, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSavingGoal([FromBody] UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(request, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSavingGoal(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteSavingGoalAsync(id, cancellationToken);
            return NoContent();
        }
    }
}