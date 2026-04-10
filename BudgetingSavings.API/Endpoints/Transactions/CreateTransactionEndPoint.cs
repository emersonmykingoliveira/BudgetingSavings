using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;

namespace BudgetingSavings.API.Endpoints.Transactions
{
    public class CreateSavingGoalEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/createtransaction", async (HttpContext context, CreateTransactionRequest request) =>
            {

            });
        }
    }
}
