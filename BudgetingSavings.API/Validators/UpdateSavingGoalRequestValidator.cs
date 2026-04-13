using BudgetingSavings.Shared.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class UpdateSavingGoalRequestValidator : AbstractValidator<UpdateSavingGoalRequest>
    {
        public UpdateSavingGoalRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.TargetAmount)
                .GreaterThan(0);

            RuleFor(x => x.TargetDate)
                .GreaterThan(DateTime.UtcNow);
        }
    }
}
