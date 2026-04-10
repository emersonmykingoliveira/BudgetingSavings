using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController(ITransactionsService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions(CancellationToken cancellationToken)
        {
            var transactions = await service.GetAllTransactionsAsync(cancellationToken);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id, CancellationToken cancellationToken)
        {
            var transaction = await service.GetTransactionAsync(id, cancellationToken);
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            return Ok();
        }
    }
}
