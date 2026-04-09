using BudgetingSavings.API.Features;

namespace BudgetingSavings.API.Endpoints.SavingGoals
{
    public class CreateSavingGoalEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/createsavinggoal", async (HttpContext context, CreateSavingGoalRequest request) =>
            {

            }).RequireAuthorization("documentPolicy");
        }
    }
}
