using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:guid}/rewards")]
    public class RewardsController(IRewardService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllRewards(Guid customerId, CancellationToken cancellationToken)
        {
            var rewards = await service.GetAllRewardsAsync(customerId, cancellationToken);
            return Ok(rewards);
        }

        [HttpGet("{rewardId:guid}")]
        public async Task<IActionResult> GetReward(Guid customerId, Guid rewardId, CancellationToken cancellationToken)
        {
            var reward = await service.GetRewardAsync(rewardId, customerId, cancellationToken);
            return Ok(reward);
        }

        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemReward(Guid customerId, [FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
        {
            request.CustomerId = customerId;
            var reward = await service.RedeemRewardAsync(request, cancellationToken);
            return Ok(reward);
        }
    }
}