using FlagForge.Data.ViewModels;
using FluentValidation;

namespace FlagForge.Data.Validations;

public class CreateEnvironmentRequestValidator : AbstractValidator<CreateEnvironmentRequest>
{
    public CreateEnvironmentRequestValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tenant id is required.");
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Environment name is required.");
    }
}
