using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using BudgetingSavings.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/saving-goals")]
    [EnableRateLimiting("fixedRateLimiter")]
    [Authorize]
    public class SavingGoalsController(ISavingGoalService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all saving goals for a customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the list of saving goals.</response>
        /// <response code="400">If the customer does not exist or an error occurs.</response>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(List<SavingGoalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllSavingGoals(Guid customerId, CancellationToken cancellationToken)
        {
            var result = await service.GetAllSavingGoalsAsync(customerId, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves saving suggestions for a customer based on their spending and goals.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the saving suggestions.</response>
        /// <response code="400">If the customer does not exist.</response>
        [HttpGet("customer/{customerId:guid}/suggestions")]
        [ProducesResponseType(typeof(SavingSuggestionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSuggestions(Guid customerId, CancellationToken cancellationToken)
        {
            var result = await service.GetSavingSuggestions(customerId, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific saving goal by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the saving goal.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the requested saving goal details.</response>
        /// <response code="400">If the saving goal does not exist.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SavingGoalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSavingGoalById(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetSavingGoalByIdAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the current status and progress of a saving goal.
        /// </summary>
        /// <param name="id">The unique identifier of the saving goal.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the status and progress of the saving goal.</response>
        /// <response code="400">If the saving goal does not exist.</response>
        [HttpGet("{id:guid}/status")]
        [ProducesResponseType(typeof(SavingGoalStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSavingGoalStatus(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetSavingGoalStatusAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new saving goal.
        /// </summary>
        /// <param name="request">The saving goal creation details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="201">Returns the newly created saving goal.</response>
        /// <response code="400">If the request is invalid, customer does not exist, or active goals limit reached.</response>
        [HttpPost]
        [ProducesResponseType(typeof(SavingGoalResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var result = await service.CreateSavingGoalAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetSavingGoalById), new { id = result.Value?.Id }, result.Value);
        }

        /// <summary>
        /// Updates an existing saving goal's details.
        /// </summary>
        /// <param name="request">The saving goal update details.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="200">Returns the updated saving goal details.</response>
        /// <response code="400">If the request is invalid or the saving goal does not exist.</response>
        [HttpPut]
        [ProducesResponseType(typeof(SavingGoalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSavingGoal([FromBody] UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var result = await service.UpdateSavingGoalAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a saving goal.
        /// </summary>
        /// <param name="id">The unique identifier of the saving goal to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <response code="204">If the saving goal was successfully deleted.</response>
        /// <response code="400">If the saving goal does not exist.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSavingGoal(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.DeleteSavingGoalAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
    }
}
