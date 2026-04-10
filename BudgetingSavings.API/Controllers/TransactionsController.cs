using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController() : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            return Ok();
        }
    }
}
