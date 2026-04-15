using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/rewards")]
    [EnableRateLimiting("fixedRateLimiter")]
    public class RewardsController(IRewardService service) : ControllerBase
    {
        /// <summary>
        /// Retrieves all available rewards for a customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of rewards.</returns>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(List<RewardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllRewards(Guid customerId, CancellationToken cancellationToken)
        {
            var result = await service.GetAllRewardsAsync(customerId, cancellationToken);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific reward by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the reward.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The reward details.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RewardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRewardById(Guid id, CancellationToken cancellationToken)
        {
            var result = await service.GetRewardByIdAsync(id, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Redeems a reward for a customer.
        /// </summary>
        /// <param name="request">The reward redemption details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the redemption.</returns>
        [HttpPost("redeem")]
        [ProducesResponseType(typeof(RedeemRewardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
        {
            var result = await service.RedeemRewardAsync(request, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }
    }
}
