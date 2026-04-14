using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController(ICustomerService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of customers.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCustomers(CancellationToken cancellationToken)
        {
            var customers = await service.GetAllCustomersAsync(cancellationToken);
            return Ok(customers);
        }

        /// <summary>
        /// Retrieves a specific customer by their identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The customer details.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
        {
            var customer = await service.GetCustomerByIdAsync(id, cancellationToken);
            return Ok(customer);
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="request">The customer creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created customer.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.CreateCustomerAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        /// <summary>
        /// Updates an existing customer's details.
        /// </summary>
        /// <param name="request">The customer update details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated customer details.</returns>
        [HttpPut]
        [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await service.UpdateCustomerAsync(request, cancellationToken);
            return Ok(customer);
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteCustomerAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
