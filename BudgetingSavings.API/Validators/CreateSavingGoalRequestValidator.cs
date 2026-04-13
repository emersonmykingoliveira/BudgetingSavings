using BudgetingSavings.Shared.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class CreateSavingGoalRequestValidator : AbstractValidator<CreateSavingGoalRequest>
    {
        public CreateSavingGoalRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.TargetAmount)
                .GreaterThan(0);

            RuleFor(x => x.TargetDate)
                .GreaterThan(DateTime.UtcNow);

            RuleFor(x => x.AccountId)
                .NotEmpty();

            RuleFor(x => x.CustomerId)
                .NotEmpty();
        }
    }
}
