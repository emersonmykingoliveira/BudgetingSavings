using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalController(ISavingGoalService service) : ControllerBase
    {
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetAllSavingGoals(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await service.GetAllSavingGoalsAsync(customerId, cancellationToken);
            return Ok(savingGoals);
        }

        [HttpGet("{customerId}/{id}")]
        public async Task<IActionResult> GetSavingGoal(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await service.GetSavingGoalAsync(customerId, id, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request)
        {
            var savingGoal = await service.CreateSavingGoalAsync(request, CancellationToken.None);
            return Ok(savingGoal);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSavingGoal([FromBody] UpdateSavingGoalRequest request)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(request, CancellationToken.None);
            return Ok(savingGoal);
        }

        [HttpDelete("{customerId}/{id}")]
        public async Task<IActionResult> DeleteSavingGoal(Guid customerId, Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteSavingGoalAsync(customerId, id, cancellationToken);
            return NoContent();
        }
    }
}
