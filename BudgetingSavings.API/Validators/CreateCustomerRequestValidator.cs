using BudgetingSavings.API.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
    {
        public CreateCustomerRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .LessThan(DateTime.UtcNow);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20);
        }
    }
}
