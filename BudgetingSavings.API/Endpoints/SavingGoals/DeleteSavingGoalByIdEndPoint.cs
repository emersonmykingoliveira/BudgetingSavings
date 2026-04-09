using BudgetingSavings.API.Features;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class DeleteSavingGoalByIdEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/savinggoal/{goalId}", async (HttpContext context, int goalId) =>
            {

            });
        }
    }
}
