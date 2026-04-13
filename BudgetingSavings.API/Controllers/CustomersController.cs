using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController(ICustomerService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers(CancellationToken cancellationToken)
        {
            var customers = await service.GetAllCustomersAsync(cancellationToken);
            return Ok(customers);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
        {
            var customer = await service.GetCustomerByIdAsync(id, cancellationToken);
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.CreateCustomerAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.UpdateCustomerAsync(request, cancellationToken);
            return Ok(customer);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteCustomerAsync(id, cancellationToken);
            return NoContent();
        }
    }
}