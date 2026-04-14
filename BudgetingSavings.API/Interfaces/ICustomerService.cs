using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

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
