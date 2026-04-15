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
    public class BudgetsController(IBudgetService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all budgets for a customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of budgets.</returns>
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The budget details.</returns>
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The status and progress of the budget.</returns>
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created budget.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateBudgetAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetBudgetById), new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Updates an existing budget's details.
        /// </summary>
        /// <param name="request">The budget update details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated budget details.</returns>
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
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
