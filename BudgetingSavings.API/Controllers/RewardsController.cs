using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/rewards")]
    public class RewardsController(IRewardService service) : ControllerBase
    {
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetAllRewards(Guid customerId, CancellationToken cancellationToken)
        {
            var rewards = await service.GetAllRewardsAsync(customerId, cancellationToken);
            return Ok(rewards);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRewardById(Guid id, CancellationToken cancellationToken)
        {
            var reward = await service.GetRewardByIdAsync(id, cancellationToken);
            return Ok(reward);
        }

        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
        {
            var reward = await service.RedeemRewardAsync(request, cancellationToken);
            return Ok(reward);
        }
    }
}