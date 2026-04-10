using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(ApiDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await db.Accounts.ToListAsync();
            return Ok(accounts);
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
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountRequest request)
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
