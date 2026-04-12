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


        [HttpGet("{id}/customer/{customerId}/Status")]
        public async Task<IActionResult> GetSavingGoalStatus(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoalStatus = await service.GetSavingGoalStatusAsync(id, customerId, cancellationToken);
            return Ok(savingGoalStatus);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.CreateSavingGoalAsync(request, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingGoal(Guid Id, [FromBody] UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(Id, request, cancellationToken);
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
