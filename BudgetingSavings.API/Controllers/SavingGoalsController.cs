using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalsController() : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSavingGoals()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSavingGoal(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSavingGoal([FromBody] object savingGoal)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingGoal(int id, [FromBody] object savingGoal)
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
