using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController(IAccountService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts(CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsAsync(cancellationToken);
            return Ok(accounts);
        }

        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetAllAccountsForCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsForCustomerAsync(customerId, cancellationToken);
            return Ok(accounts);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken)
        {
            var account = await service.GetAccountByIdAsync(id, cancellationToken);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = await service.CreateAccountAsync(request, cancellationToken);
            return Ok(account);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteAccountAsync(id, cancellationToken);
            return NoContent();
        }
    }
}