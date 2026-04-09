using BudgetingSavings.API.Features;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class DeleteAllSavingGoalsByIdEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/savinggoals", async (HttpContext context) =>
            {

            });
        }
    }
}
