using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountsService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts(CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsAsync(cancellationToken);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(CancellationToken cancellationToken, Guid id)
        {
            var account = await service.GetAccountAsync(cancellationToken, id);
            return Ok(account);
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
