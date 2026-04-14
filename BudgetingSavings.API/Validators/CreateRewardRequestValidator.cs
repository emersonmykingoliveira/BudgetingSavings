using BudgetingSavings.BusinessLayer.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.BusinessLayer.Validators
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
