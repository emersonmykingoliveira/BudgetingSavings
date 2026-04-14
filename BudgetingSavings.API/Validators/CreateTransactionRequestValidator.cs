using BudgetingSavings.BusinessLayer.Models.Requests;
using FluentValidation;

namespace BudgetingSavings.BusinessLayer.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.AccountId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.TransactionType)
            .IsInEnum();

        RuleFor(x => x.TransactionCategory)
            .IsInEnum();

        RuleFor(x => x.Currency)
            .IsInEnum();
    }
}