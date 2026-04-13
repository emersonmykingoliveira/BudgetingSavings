using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController(ITransactionService service) : ControllerBase
    {
        [HttpGet("account/{accountId:guid}")]
        public async Task<IActionResult> GetAccountTransactions(Guid accountId, CancellationToken cancellationToken)
        {
            var transactions = await service.GetAllTransactionsAsync(accountId, cancellationToken);
            return Ok(transactions);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTransaction(Guid id, CancellationToken cancellationToken)
        {
            var transaction = await service.GetTransactionByIdAsync(id, cancellationToken);
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var transaction = await service.CreateTransactionAsync(request, cancellationToken);
            return Ok(transaction);
        }
    }
}