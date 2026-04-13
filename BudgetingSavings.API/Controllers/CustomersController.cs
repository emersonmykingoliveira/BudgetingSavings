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

        [HttpGet("{customerId:guid}")]
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

        [HttpPut("{customerId:guid}")]
        public async Task<IActionResult> UpdateCustomer(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.UpdateCustomerAsync(customerId, request, cancellationToken);
            return Ok(customer);
        }

        [HttpDelete("{customerId:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid customerId, CancellationToken cancellationToken)
        {
            await service.DeleteCustomerAsync(customerId, cancellationToken);
            return NoContent();
        }
    }
}