using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalsController(ISavingGoalsService service) : ControllerBase
    {
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAllSavingGoals(Guid accountId, CancellationToken cancellationToken)
        {
            var savingGoals = await service.GetAllSavingGoalsAsync(accountId, cancellationToken);
            return Ok(savingGoals);
        }

        [HttpGet("{accountId}/{id}")]
        public async Task<IActionResult> GetSavingGoal(Guid accountId, Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await service.GetSavingGoalAsync(accountId, id, cancellationToken);
            return Ok(savingGoal);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request)
        {
            var savingGoal = await service.CreateSavingGoalAsync(request, CancellationToken.None);
            return Ok(savingGoal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingGoal([FromBody] UpdateSavingGoalRequest request)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(request, CancellationToken.None);
            return Ok(savingGoal);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavingGoal(Guid accountId, Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteSavingGoalAsync(accountId, id, cancellationToken);
            return Ok();
        }
    }
}
