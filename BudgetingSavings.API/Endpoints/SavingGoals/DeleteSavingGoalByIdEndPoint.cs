using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class DeleteSavingGoalsByIdEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/savinggoal/{goalId}", async (HttpContext context, int goalId) =>
            {

            });
        }
    }
}
