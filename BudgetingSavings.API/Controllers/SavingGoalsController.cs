using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalsController() : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSavingGoals()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetSavingGoal(int id)
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult CreateSavingGoal([FromBody] object savingGoal)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSavingGoal(int id, [FromBody] object savingGoal)
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
