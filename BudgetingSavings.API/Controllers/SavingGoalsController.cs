using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:guid}/saving-goals")]
    public class SavingGoalsController(ISavingGoalService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllSavingGoals(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await service.GetAllSavingGoalsAsync(customerId, cancellationToken);
            return Ok(savingGoals);
        }

        [HttpGet("{savingGoalId:guid}")]
        public async Task<IActionResult> GetSavingGoal(Guid customerId, Guid savingGoalId, CancellationToken cancellationToken)
        {
            var savingGoal = await service.GetSavingGoalAsync(savingGoalId, customerId, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpGet("{savingGoalId:guid}/status")]
        public async Task<IActionResult> GetSavingGoalStatus(Guid customerId, Guid savingGoalId, CancellationToken cancellationToken)
        {
            var savingGoalStatus = await service.GetSavingGoalStatusAsync(savingGoalId, customerId, cancellationToken);
            return Ok(savingGoalStatus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal(Guid customerId, [FromBody] CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            request.CustomerId = customerId;
            var savingGoal = await service.CreateSavingGoalAsync(request, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpPut("{savingGoalId:guid}")]
        public async Task<IActionResult> UpdateSavingGoal(Guid customerId, Guid savingGoalId, [FromBody] UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(savingGoalId, request, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpDelete("{savingGoalId:guid}")]
        public async Task<IActionResult> DeleteSavingGoal(Guid customerId, Guid savingGoalId, CancellationToken cancellationToken)
        {
            await service.DeleteSavingGoalAsync(savingGoalId, customerId, cancellationToken);
            return NoContent();
        }
    }
}