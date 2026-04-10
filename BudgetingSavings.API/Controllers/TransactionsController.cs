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
        public async Task<IActionResult> GetTransactionById(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] object transactionDto)
        {
            return Ok();
        }
    }
}
