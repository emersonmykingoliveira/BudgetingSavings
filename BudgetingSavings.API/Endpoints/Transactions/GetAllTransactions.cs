using BudgetingSavings.API.Features;

namespace BudgetingSavings.API.Endpoints.Transactions
{
    public class GetAllTransactions : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/transactions", async (HttpContext context) =>
            {

            });
        }
    }
}
