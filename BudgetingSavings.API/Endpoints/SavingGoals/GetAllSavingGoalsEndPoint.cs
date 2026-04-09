using BudgetingSavings.API.Features;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class GetAllSavingGoalsEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/savinggoals", async (HttpContext context) =>
            {

            });
        }
    }
}
