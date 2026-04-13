using BudgetingSavings.Shared.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class CreateRewardRequestValidator : AbstractValidator<CreateRewardRequest>
    {
        public CreateRewardRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty();

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.TransactionType)
                .IsInEnum();

            RuleFor(x => x.TransactionCategory)
                .IsInEnum();
        }
    }
}
