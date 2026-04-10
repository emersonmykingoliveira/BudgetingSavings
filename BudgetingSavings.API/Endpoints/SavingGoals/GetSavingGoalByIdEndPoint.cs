using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class GetSavingGoalByIdEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/savinggoal/{goalId}", async (HttpContext context, int goalId) =>
            {

            });
        }
    }
}
