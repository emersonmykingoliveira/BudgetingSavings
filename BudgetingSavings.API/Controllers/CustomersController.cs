using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController(ICustomerService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllCustomer(CancellationToken cancellationToken)
        {
            var customers = await service.GetAllCustomersAsync(cancellationToken);
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(Guid id, CancellationToken cancellationToken)
        {
            var customer = await service.GetCustomerAsync(id, cancellationToken);
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.CreateCustomerAsync(request, cancellationToken);
            return Ok(customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.UpdateCustomerAsync(id, request, cancellationToken);
            return Ok(customer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteCustomerAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
