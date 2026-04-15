using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;

namespace BudgetingSavings.API.Interfaces
{
    public interface ICustomerService
    {
        Task<Result<List<CustomerResponse>>> GetAllCustomersAsync(CancellationToken cancellationToken);
        Task<Result<CustomerResponse>> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
        Task<Result> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<CustomerResponse>> UpdateCustomerAsync(UpdateCustomerRequest request, CancellationToken cancellationToken);
    }
}
