using FluentValidation;
using MaverickBank.Core.DTOs;

namespace MaverickBank.API.Validators
{
    public class LoanApplyDTOValidator : AbstractValidator<LoanApplyDTO>
    {
        public LoanApplyDTOValidator()
        {
            RuleFor(x => x.AccountId)
                .GreaterThan(0);

            RuleFor(x => x.LoanProductId)
                .GreaterThan(0);

            RuleFor(x => x.AmountApplied)
                .GreaterThanOrEqualTo(1000)
                .WithMessage("Loan amount must be at least 1000.");

            RuleFor(x => x.Purpose)
                .MaximumLength(200);
        }
    }
}
