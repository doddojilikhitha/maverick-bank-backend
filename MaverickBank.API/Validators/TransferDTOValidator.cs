using FluentValidation;
using MaverickBank.Core.DTOs;

namespace MaverickBank.API.Validators
{
    public class TransferDTOValidator : AbstractValidator<TransferDTO>
    {
        public TransferDTOValidator()
        {
            RuleFor(x => x.FromAccountId)
                .GreaterThan(0);

            RuleFor(x => x.ToAccountId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}