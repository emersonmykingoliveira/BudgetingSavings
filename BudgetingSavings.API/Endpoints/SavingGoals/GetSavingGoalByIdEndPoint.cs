using BudgetingSavings.API.Features;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class GetSavingGoalByIdEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/savinggoal/{goalId}", async (HttpContext context, int goalId) =>
            {

            });
        }
    }
}
