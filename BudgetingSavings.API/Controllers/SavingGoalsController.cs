using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalsController(ISavingGoalsService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllSavingGoals()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSavingGoal(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingGoal(int id, [FromBody] UpdateSavingGoalRequest request)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSavingGoal(int id)
        {
            return Ok();
        }
    }
}
