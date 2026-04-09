using BudgetingSavings.API.Features;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class UpdateSavingGoalEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/updatesavinggoal", async (HttpContext context, UpdateSavingGoalRequest request) =>
            {

            });
        }
    }
}
