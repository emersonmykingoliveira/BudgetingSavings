using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController(ITransactionService service) : ControllerBase
    {
        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetAccountTransactions(Guid accountId, CancellationToken cancellationToken)
        {
            var transactions = await service.GetAllTransactionsAsync(accountId, cancellationToken);
            return Ok(transactions);
        }

        [HttpGet("{id}/account/{accountId}")]
        public async Task<IActionResult> GetTransaction(Guid id, Guid accountId, CancellationToken cancellationToken)
        {
            var transaction = await service.GetTransactionAsync(id, accountId, cancellationToken);
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
