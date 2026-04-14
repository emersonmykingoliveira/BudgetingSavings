using BudgetingSavings.API.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
    {
        public UpdateBudgetRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.StartTime)
                .NotEmpty();

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime);

            RuleFor(x => x.LimitAmount)
                .GreaterThan(0);

            RuleFor(x => x.Currency)
                .IsInEnum();
        }
    }
}
