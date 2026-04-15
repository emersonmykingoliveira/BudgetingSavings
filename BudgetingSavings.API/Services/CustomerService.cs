using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BudgetingSavings.API.Services
{
    public class CustomerService(ApiDbContext db, 
                                IValidator<CreateCustomerRequest> createValidator,
                                IValidator<UpdateCustomerRequest> updateValidator) : ICustomerService
    {
        public async Task<Result<CustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var emailExists = await db.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken);
            
            if (emailExists)
                return Result<CustomerResponse>.Fail("A customer with this email already exists.");

            var phoneExists = await db.Customers.AnyAsync(c => c.PhoneNumber == request.PhoneNumber, cancellationToken);
            
            if (phoneExists)
                return Result<CustomerResponse>.Fail("A customer with this phone number already exists.");

            if (request.DateOfBirth > DateTime.UtcNow.AddYears(-18))
                return Result<CustomerResponse>.Fail("Customer must be at least 18 years old.");

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
            return await MapCustomerResponse(customer);
        }

        public async Task<Result> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = await GetSpecificCustomerAsync(id, cancellationToken);

            if (customer is null)
                return Result.Fail("Customer does not exist.");

            var hasAccounts = await db.Accounts.AnyAsync(a => a.CustomerId == id, cancellationToken);
            
            if (hasAccounts)
                return Result.Fail("Cannot delete a customer with existing accounts.");

            db.Customers.Remove(customer);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<List<CustomerResponse>>> GetAllCustomersAsync(CancellationToken cancellationToken)
        {
            var customers = await db.Customers.ToListAsync(cancellationToken);
            var customerResponses = new List<CustomerResponse>();

            foreach (var customer in customers)
            {
                var responseResult = await MapCustomerResponse(customer);
                if (responseResult.Value != null)
                {
                    customerResponses.Add(responseResult.Value);
                }
            }

            return Result<List<CustomerResponse>>.Success(customerResponses);
        }

        public async Task<Result<CustomerResponse>> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var customer = await GetSpecificCustomerAsync(id, cancellationToken);
            
            if(customer is null)
                return Result<CustomerResponse>.Fail("Customer does not exist.");

            return await MapCustomerResponse(customer);
        }

        private async Task<Customer?> GetSpecificCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            return await db.Customers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<Result<CustomerResponse>> UpdateCustomerAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var customer = await GetSpecificCustomerAsync(request.Id, cancellationToken);

            if (customer is null)
                return Result<CustomerResponse>.Fail("Customer does not exist.");

            var emailExists = await db.Customers.AnyAsync(c => c.Id != request.Id && c.Email == request.Email, cancellationToken);
            
            if (emailExists)
                return Result<CustomerResponse>.Fail("A customer with this email already exists.");

            var phoneExists = await db.Customers.AnyAsync(c => c.Id != request.Id && c.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (phoneExists)
                return Result<CustomerResponse>.Fail("A customer with this phone number already exists.");

            if (request.DateOfBirth > DateTime.UtcNow.AddYears(-18))
                return Result<CustomerResponse>.Fail("Customer must be at least 18 years old.");

            customer.Name = request.Name;
            customer.DateOfBirth = request.DateOfBirth;
            customer.PhoneNumber = request.PhoneNumber;
            customer.Email = request.Email;

            db.Customers.Update(customer);
            await db.SaveChangesAsync(cancellationToken);

            return await MapCustomerResponse(customer);
        }

        private Task<Result<CustomerResponse>> MapCustomerResponse(Customer customer)
        {
            var response = new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                DateOfBirth = customer.DateOfBirth,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email
            };

            return Task.FromResult(Result<CustomerResponse>.Success(response));
        }
    }
}
