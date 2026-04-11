using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Services
{
    public class CustomerService(ApiDbContext db) : ICustomerService
    {
        public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email
            };

            await db.Customers.AddAsync(customer, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return MapCustomerResponse(customer);
        }

        public async Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (customer is not null)
            {
                db.Customers.Remove(customer);
                await db.SaveChangesAsync(cancellationToken);
            }
            //todo: handle not found case
        }

        public async Task<List<CustomerResponse>> GetAllCustomersAsync(CancellationToken cancellationToken)
        {
            var customers = await db.Customers.ToListAsync(cancellationToken);
            return customers.Select(MapCustomerResponse).ToList();
        }

        public async Task<CustomerResponse> GetCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = await GetSpecificCustomerAsync(id, cancellationToken);
            return MapCustomerResponse(customer);
        }

        private async Task<Customer> GetSpecificCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Customers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new Customer();
        }

        public async Task<CustomerResponse> UpdateCustomerAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await GetSpecificCustomerAsync(request.Id, cancellationToken);

            if(customer is not null)
            {
                customer.Name = request.Name;
                customer.DateOfBirth = request.DateOfBirth;
                customer.PhoneNumber = request.PhoneNumber;
                customer.Email = request.Email;

                db.Customers.Update(customer);
                await db.SaveChangesAsync(cancellationToken);
            }

            return MapCustomerResponse(customer);
        }

        private CustomerResponse MapCustomerResponse(Customer? customer)
        {
            if(customer is null) return new CustomerResponse();

            return new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                DateOfBirth = customer.DateOfBirth,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email
            };
        }
    }
}
