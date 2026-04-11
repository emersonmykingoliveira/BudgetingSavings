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

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            var customer = await service.GetCustomerAsync(customerId, cancellationToken);
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.CreateCustomerAsync(request, cancellationToken);
            return Ok(customer);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.UpdateCustomerAsync(request, cancellationToken);
            return Ok(customer);
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            await service.DeleteCustomerAsync(customerId, cancellationToken);
            return NoContent();
        }
    }
}
