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
    public class SavingGoalsController(ISavingGoalService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all saving goals for a customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of saving goals.</returns>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(List<SavingGoalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllSavingGoals(Guid customerId, CancellationToken cancellationToken)
        {
            var savingGoals = await service.GetAllSavingGoalsAsync(customerId, cancellationToken);
            return Ok(savingGoals);
        }

        /// <summary>
        /// Retrieves saving suggestions for a customer based on their spending and goals.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Saving suggestions.</returns>
        [HttpGet("customer/{customerId:guid}/suggestions")]
        [ProducesResponseType(typeof(SavingSuggestionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSuggestions(Guid customerId, CancellationToken cancellationToken)
        {
            var savingSuggestions = await service.GetSavingSuggestions(customerId, cancellationToken);
            return Ok(savingSuggestions);
        }

        /// <summary>
        /// Retrieves a specific saving goal by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the saving goal.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The saving goal details.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SavingGoalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSavingGoalById(Guid id, CancellationToken cancellationToken)
        {
            var savingGoal = await service.GetSavingGoalByIdAsync(id, cancellationToken);
            return Ok(savingGoal);
        }

        /// <summary>
        /// Retrieves the current status and progress of a saving goal.
        /// </summary>
        /// <param name="id">The unique identifier of the saving goal.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The status and progress of the saving goal.</returns>
        [HttpGet("{id:guid}/status")]
        [ProducesResponseType(typeof(SavingGoalStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSavingGoalStatus(Guid id, CancellationToken cancellationToken)
        {
            var savingGoalStatus = await service.GetSavingGoalStatusAsync(id, cancellationToken);
            return Ok(savingGoalStatus);
        }

        /// <summary>
        /// Creates a new saving goal.
        /// </summary>
        /// <param name="request">The saving goal creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The newly created saving goal.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SavingGoalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSavingGoal([FromBody] CreateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.CreateSavingGoalAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSavingGoalById), new { id = savingGoal.Id }, savingGoal);
        }

        /// <summary>
        /// Updates an existing saving goal's details.
        /// </summary>
        /// <param name="request">The saving goal update details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated saving goal details.</returns>
        [HttpPut]
        [ProducesResponseType(typeof(SavingGoalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSavingGoal([FromBody] UpdateSavingGoalRequest request, CancellationToken cancellationToken)
        {
            var savingGoal = await service.UpdateSavingGoalAsync(request, cancellationToken);
            return Ok(savingGoal);
        }

        /// <summary>
        /// Deletes a saving goal.
        /// </summary>
        /// <param name="id">The unique identifier of the saving goal to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSavingGoal(Guid id, CancellationToken cancellationToken)
        {
            await service.DeleteSavingGoalAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
