using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/rewards")]
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
            var rewards = await service.GetAllRewardsAsync(customerId, cancellationToken);
            return Ok(rewards);
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
            var reward = await service.GetRewardByIdAsync(id, cancellationToken);
            return Ok(reward);
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
            var reward = await service.RedeemRewardAsync(request, cancellationToken);
            return Ok(reward);
        }
    }
}
