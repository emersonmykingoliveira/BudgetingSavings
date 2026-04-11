using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController(IBudgetingService service) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
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
