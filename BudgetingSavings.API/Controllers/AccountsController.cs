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
    [Authorize]
    public class AccountsController(IAccountService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all customer accounts.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the list of accounts.</response>
        /// <response code="400">If an error occurs while retrieving accounts.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<AccountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAccounts(CancellationToken cancellationToken)
        {
            var result = await service.GetAllAccountsAsync(cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves all accounts for a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the list of customer accounts.</response>
        /// <response code="400">If the customer does not exist or an error occurs.</response>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(List<AccountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAccountsForCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            var result = await service.GetAllAccountsForCustomerAsync(customerId, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific account by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the account.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the requested account.</response>
        /// <response code="400">If the account does not exist.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAccountById(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetAccountByIdAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="request">The account creation details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="201">Returns the newly created account.</response>
        /// <response code="400">If the request is invalid or the customer already has this type of account.</response>
        [HttpPost]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateAccountAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetAccountById), new { id = result.Value?.Id }, result.Value);
        }

        /// <summary>
        /// Deletes an account.
        /// </summary>
        /// <param name="id">The unique identifier of the account to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="204">If the account was successfully deleted.</response>
        /// <response code="400">If the account cannot be deleted due to balance or transaction history.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.DeleteAccountAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}