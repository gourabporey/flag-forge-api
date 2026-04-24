using FlagForge.Data.ViewModels;
using FluentValidation;

namespace FlagForge.Data.Validations;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
    }
}
