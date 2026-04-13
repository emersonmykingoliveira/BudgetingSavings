using BudgetingSavings.Shared.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
    {
        public CreateAccountRequestValidator()
        {
            RuleFor(x => x.AccountType)
                .IsInEnum();

            RuleFor(x => x.Currency)
                .IsInEnum();

            RuleFor(x => x.CustomerId)
                .NotEmpty();
        }
    }
}
