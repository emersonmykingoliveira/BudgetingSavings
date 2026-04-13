using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/accounts/{accountId:guid}/transactions")]
    public class TransactionsController(ITransactionService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAccountTransactions(Guid accountId, CancellationToken cancellationToken)
        {
            var transactions = await service.GetAllTransactionsAsync(accountId, cancellationToken);
            return Ok(transactions);
        }

        [HttpGet("{transactionId:guid}")]
        public async Task<IActionResult> GetTransaction(Guid accountId, Guid transactionId, CancellationToken cancellationToken)
        {
            var transaction = await service.GetTransactionAsync(transactionId, accountId, cancellationToken);
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(Guid accountId, [FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            request.AccountId = accountId;
            var transaction = await service.CreateTransactionAsync(request, cancellationToken);
            return Ok(transaction);
        }
    }
}