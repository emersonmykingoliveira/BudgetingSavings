using BudgetingSavings.Shared.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.API.Validators
{
    public class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
    {
        public CreateBudgetRequestValidator()
        {
            RuleFor(x => x.StartTime)
                .NotEmpty();

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime);

            RuleFor(x => x.LimitAmount)
                .GreaterThan(0);

            RuleFor(x => x.Currency)
                .IsInEnum();

            RuleFor(x => x.CustomerId)
                .NotEmpty();
        }
    }
}
