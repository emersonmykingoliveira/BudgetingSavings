using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BudgetingSavings.API.Services
{
    public class CustomerService(ApiDbContext db, 
                                IValidator<CreateCustomerRequest> createValidator,
                                IValidator<UpdateCustomerRequest> updateValidator) : ICustomerService
    {
        public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var emailExists = await db.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken);
            
            if (emailExists)
                throw new ArgumentException("A customer with this email already exists.");

            var phoneExists = await db.Customers.AnyAsync(c => c.PhoneNumber == request.PhoneNumber, cancellationToken);
            
            if (phoneExists)
                throw new ArgumentException("A customer with this phone number already exists.");

            if (request.DateOfBirth > DateTime.UtcNow.AddYears(-18))
                throw new ArgumentException("Customer must be at least 18 years old.");

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
            var customer = await GetSpecificCustomerAsync(id, cancellationToken);

            var hasAccounts = await db.Accounts.AnyAsync(a => a.CustomerId == id, cancellationToken);
            
            if (hasAccounts)
                throw new ArgumentException("Cannot delete a customer with existing accounts.");

            if (customer is not null)
            {
                db.Customers.Remove(customer);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<CustomerResponse>> GetAllCustomersAsync(CancellationToken cancellationToken)
        {
            var customers = await db.Customers.ToListAsync(cancellationToken);
            return customers.Select(MapCustomerResponse).ToList();
        }

        public async Task<CustomerResponse> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = await GetSpecificCustomerAsync(id, cancellationToken);
            return MapCustomerResponse(customer);
        }

        private async Task<Customer> GetSpecificCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken) ?? new Customer();

            if(customer is null)
                throw new ArgumentException("Customer does not exist.");

            return customer;
        }

        public async Task<CustomerResponse> UpdateCustomerAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customer = await GetSpecificCustomerAsync(request.Id, cancellationToken);

            var emailExists = await db.Customers.AnyAsync(c => c.Id != request.Id && c.Email == request.Email, cancellationToken);
            
            if (emailExists)
                throw new ArgumentException("A customer with this email already exists.");

            var phoneExists = await db.Customers.AnyAsync(c => c.Id != request.Id && c.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (phoneExists)
                throw new ArgumentException("A customer with this phone number already exists.");

            if (request.DateOfBirth > DateTime.UtcNow.AddYears(-18))
                throw new ArgumentException("Customer must be at least 18 years old.");

            if (customer is not null)
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
            if (customer is null) return new CustomerResponse();

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
