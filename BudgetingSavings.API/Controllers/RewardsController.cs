using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BudgetingSavings.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RewardsController(IRewardService service) : ControllerBase
    {
        [HttpGet("customer/{customerId}/history")]
        public async Task<IActionResult> GetAllRewards(Guid customerId, CancellationToken cancellationToken)
        {
            var rewards = await service.GetAllRewardsAsync(customerId, cancellationToken);
            return Ok(rewards);
        }

        [HttpGet("{id}/customer/{customerId}")]
        public async Task<IActionResult> GetReward(Guid id, Guid customerId, CancellationToken cancellationToken)
        {
            var reward = await service.GetRewardAsync(id, customerId, cancellationToken);
            return Ok(reward);
        }

        [HttpPost]
        public async Task<IActionResult> RedeemReward([FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
        {
            var reward = await service.RedeemRewardAsync(request, cancellationToken);
            return Ok(reward);
        }
    }
}
