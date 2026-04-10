using BudgetingSavings.API.Interfaces;
using System.Reflection;

namespace BudgetingSavings.API.Services
{
    public static class EndpointExtensions
    {
        public static IServiceCollection AddEndpointAutoDiscovery(this IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IEndpointDiscovery).IsAssignableFrom(t) && t.IsClass);
            foreach (var t in types)
                services.AddTransient(typeof(IEndpointDiscovery), t);
            return services;
        }

        public static WebApplication MapEndpoints(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var modules = scope.ServiceProvider.GetServices<IEndpointDiscovery>();

            var group = app.MapGroup("/");

            foreach (var m in modules)
                m.MapEndpoint(group);
            return app;
        }
    }
}
