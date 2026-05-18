using FluentValidation;
using MaverickBank.Core.DTOs;

namespace MaverickBank.API.Validators
{
    public class OpenAccountDTOValidator : AbstractValidator<OpenAccountDTO>
    {
        public OpenAccountDTOValidator()
        {
            RuleFor(x => x.AccountType)
                .NotEmpty()
                .WithMessage("Account type is required.");

            RuleFor(x => x.BranchName)
                .NotEmpty()
                .WithMessage("Branch name is required.");

            RuleFor(x => x.IFSCCode)
                .NotEmpty()
                .WithMessage("IFSC code is required.");

            RuleFor(x => x.BranchAddress)
                .NotEmpty()
                .WithMessage("Branch address is required.");
        }
    }
}
