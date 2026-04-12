using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts(CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsAsync(cancellationToken);
            return Ok(accounts);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetAllAccountsForCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsForCustomerAsync(customerId, cancellationToken);
            return Ok(accounts);
        }

        [HttpGet("{id}/customer/{customerId}")]
        public async Task<IActionResult> GetAccount(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var account = await service.GetAccountAsync(id, customerId, cancellationToken);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = await service.CreateAccountAsync(request, cancellationToken);
            return Ok(account);
        }

        [HttpDelete("{id}/customer/{customerId}")]
        public async Task<IActionResult> DeleteAccount(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            await service.DeleteAccountAsync(id, customerId, cancellationToken);
            return NoContent();
        }
    }
}
