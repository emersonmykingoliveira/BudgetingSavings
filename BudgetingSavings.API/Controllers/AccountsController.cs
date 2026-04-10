using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            return Ok();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateSavingGoalRequest request)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int id)
        {
            return Ok();
        }
    }
}
