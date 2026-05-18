using FluentValidation;
using MaverickBank.Core.DTOs;

namespace MaverickBank.API.Validators
{
    public class DepositWithdrawDTOValidator : AbstractValidator<DepositWithdrawDTO>
    {
        public DepositWithdrawDTOValidator()
        {
            RuleFor(x => x.AccountId)
                .GreaterThan(0)
                .WithMessage("Valid account ID is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .MaximumLength(100)
                .WithMessage("Description cannot exceed 100 characters.");
        }
    }
}