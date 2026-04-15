using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using BudgetingSavings.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [EnableRateLimiting("fixedRateLimiter")]
    public class AccountsController(IAccountService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all customer accounts.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of accounts.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AccountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAccounts(CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsAsync(cancellationToken);
            return Ok(accounts);
        }

        /// <summary>
        /// Retrieves all accounts for a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of accounts for the customer.</returns>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAccountsForCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            var accounts = await service.GetAllAccountsForCustomerAsync(customerId, cancellationToken);
            return Ok(accounts);
        }

        /// <summary>
        /// Retrieves a specific account by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The account details if found.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAccountById(Guid id, CancellationToken cancellationToken)
        {
            var account = await service.GetAccountByIdAsync(id, cancellationToken);
            return Ok(account);
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="request">The account creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created account.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var account = await service.CreateAccountAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
        }

        /// <summary>
        /// Deletes an account.
        /// </summary>
        /// <param name="id">The unique identifier of the account to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteAccountAsync(id, cancellationToken);
            return NoContent();
        }
    }
}