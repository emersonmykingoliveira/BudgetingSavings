using BudgetingSavings.API.Features;

namespace BudgetingSavings.API.Endpoints.Transactions
{
    public class GetTransactionByIdEndPoint : IEndpointDiscovery
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/transactions/{transactionId}", async (HttpContext context, int transactionId) =>
            {

            });
        }
    }
}
