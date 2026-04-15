using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using BudgetingSavings.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [EnableRateLimiting("fixedRateLimiter")]
    [Authorize]
    public class TransactionsController(ITransactionService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all transactions for a specific account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the list of transactions.</response>
        /// <response code="400">If the account does not exist or an error occurs.</response>
        [HttpGet("account/{accountId:guid}")]
        [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAccountTransactions(Guid accountId, CancellationToken cancellationToken)
        {
            var result = await service.GetAllTransactionsAsync(accountId, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific transaction by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the transaction.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the requested transaction details.</response>
        /// <response code="400">If the transaction does not exist.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionById(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetTransactionByIdAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new transaction (Debit, Credit).
        /// </summary>
        /// <param name="request">The transaction creation details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="201">Returns the newly created transaction.</response>
        /// <response code="400">If the request is invalid or insufficient balance for debit.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateTransactionAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetTransactionById), new { id = result.Value?.Id }, result.Value);
        }

        /// <summary>
        /// Creates a new transfer between two accounts.
        /// </summary>
        /// <param name="request">The transfer creation details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="201">Returns the newly created transfer transactions.</response>
        /// <response code="400">If the request is invalid, currency mismatch, or insufficient balance.</response>
        [HttpPost("Transfer")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateTransferAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetTransactionById), new { id = result.Value?.Id }, result.Value);
        }
    }
}
