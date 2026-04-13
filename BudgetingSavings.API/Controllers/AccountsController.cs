using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:guid}/accounts")]
    public class AccountsController(IAccountService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllAccountsForCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsForCustomerAsync(customerId, cancellationToken);
            return Ok(accounts);
        }

        [HttpGet("{accountId:guid}")]
        public async Task<IActionResult> GetAccount(Guid customerId, Guid accountId, CancellationToken cancellationToken)
        {
            var account = await service.GetAccountAsync(accountId, customerId, cancellationToken);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(Guid customerId, [FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
        {
            request.CustomerId = customerId;
            var account = await service.CreateAccountAsync(request, cancellationToken);
            return Ok(account);
        }

        [HttpDelete("{accountId:guid}")]
        public async Task<IActionResult> DeleteAccount(Guid customerId, Guid accountId, CancellationToken cancellationToken)
        {
            await service.DeleteAccountAsync(accountId, customerId, cancellationToken);
            return NoContent();
        }
    }
}