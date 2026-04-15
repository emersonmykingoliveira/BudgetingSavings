using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [EnableRateLimiting("fixedRateLimiter")]
    [Authorize]
    public class CustomersController(ICustomerService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the list of customers.</response>
        /// <response code="400">If an error occurs while retrieving customers.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCustomers(CancellationToken cancellationToken)
        {
            var result = await service.GetAllCustomersAsync(cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific customer by their identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the requested customer details.</response>
        /// <response code="400">If the customer does not exist.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetCustomerByIdAsync(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="request">The customer creation details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="201">Returns the newly created customer.</response>
        /// <response code="400">If the request is invalid, under 18 or the email already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateCustomerAsync(request, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Value!.Id }, result.Value);
        }

        /// <summary>
        /// Updates an existing customer's details.
        /// </summary>
        /// <param name="request">The customer update details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the updated customer details.</response>
        /// <response code="400">If the request is invalid or the customer does not exist.</response>
        [HttpPut]
        [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var result = await service.UpdateCustomerAsync(request, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The unique identifier of the customer to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="204">If the customer was successfully deleted.</response>
        /// <response code="400">If the customer cannot be deleted due to existing accounts.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.DeleteCustomerAsync(id, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
