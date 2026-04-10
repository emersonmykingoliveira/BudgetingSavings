using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    public class BudgetingController : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] object savingGoal)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            return Ok();
        }
    }
}
