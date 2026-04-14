using BudgetingSavings.BusinessLayer.Services;
using BudgetingSavings.BusinessLayer.Models.Requests;
using BudgetingSavings.BusinessLayer.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.BusinessLayer.Controllers
{
    [ApiController]
    [Route("api/budgets")]
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
            var budgets = await service.GetAllBudgetsAsync(customerId, cancellationToken);
            return Ok(budgets);
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
            var budget = await service.GetBudgetByIdAsync(id, cancellationToken);
            return Ok(budget);
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
            var budgetStatus = await service.GetBudgetStatusAsync(id, cancellationToken);
            return Ok(budgetStatus);
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
            var budget = await service.CreateBudgetAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetBudgetById), new { id = budget.Id }, budget);
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
            var budget = await service.UpdateBudgetAsync(request, cancellationToken);
            return Ok(budget);
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
            await service.DeleteBudgetAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
