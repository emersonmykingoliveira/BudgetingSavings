using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using BudgetingSavings.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    [EnableRateLimiting("fixedRateLimiter")]
    [Authorize]
    public class BudgetsController(IBudgetService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all budgets for a customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the list of budgets.</response>
        /// <response code="400">If the customer does not exist or an error occurs.</response>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(List<BudgetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBudgets(Guid customerId, CancellationToken cancellationToken)
        {
            var result = await service.GetAllBudgetsAsync(customerId, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific budget by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the budget.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the requested budget details.</response>
        /// <response code="400">If the budget does not exist.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBudgetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetBudgetByIdAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the current status and spending progress of a budget.
        /// </summary>
        /// <param name="id">The unique identifier of the budget.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the status and progress of the budget.</response>
        /// <response code="400">If the budget does not exist.</response>
        [HttpGet("{id:guid}/status")]
        [ProducesResponseType(typeof(BudgetStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBudgetStatus(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetBudgetStatusAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new budget.
        /// </summary>
        /// <param name="request">The budget creation details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="201">Returns the newly created budget.</response>
        /// <response code="400">If the request is invalid or the customer already has a budget for this period.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateBudgetAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetBudgetById), new { id = result.Value?.Id }, result.Value);
        }

        /// <summary>
        /// Updates an existing budget's details.
        /// </summary>
        /// <param name="request">The budget update details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the updated budget details.</response>
        /// <response code="400">If the request is invalid or the budget does not exist.</response>
        [HttpPut]
        [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            var result = await service.UpdateBudgetAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a budget.
        /// </summary>
        /// <param name="id">The unique identifier of the budget to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="204">If the budget was successfully deleted.</response>
        /// <response code="400">If the budget does not exist.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteBudget(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.DeleteBudgetAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
