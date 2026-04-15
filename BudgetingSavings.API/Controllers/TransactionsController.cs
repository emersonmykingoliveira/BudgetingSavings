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
    public class TransactionsController(ITransactionService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all transactions for a specific account.
        /// </summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of transactions.</returns>
        [HttpGet("account/{accountId:guid}")]
        [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAccountTransactions(Guid accountId, CancellationToken cancellationToken)
        {
            var transactions = await service.GetAllTransactionsAsync(accountId, cancellationToken);
            return Ok(transactions);
        }

        /// <summary>
        /// Retrieves a specific transaction by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the transaction.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The transaction details.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionById(Guid id, CancellationToken cancellationToken)
        {
            var transaction = await service.GetTransactionByIdAsync(id, cancellationToken);
            return Ok(transaction);
        }

        /// <summary>
        /// Creates a new transaction (Debit, Credit).
        /// </summary>
        /// <param name="request">The transaction creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created transaction.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var transaction = await service.CreateTransactionAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
        }

        /// <summary>
        /// Creates a new transfer between two accounts.
        /// </summary>
        /// <param name="request">The transfer creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created transfer transaction.</returns>
        [HttpPost("Transfer")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
        {
            var transfer = await service.CreateTransferAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTransactionById), new { id = transfer.Id }, transfer);
        }
    }
}
