using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;

namespace BudgetingSavings.API.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerResponse>> GetAllCustomersAsync(CancellationToken cancellationToken);
        Task<CustomerResponse> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
        Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken);
        Task<CustomerResponse> UpdateCustomerAsync(UpdateCustomerRequest request, CancellationToken cancellationToken);
    }
}
