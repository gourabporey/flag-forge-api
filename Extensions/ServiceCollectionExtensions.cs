using FlagForge.Data.Validations;
using FlagForge.Data.ViewModels;
using FluentValidation;

namespace FlagForge.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateEnvironmentRequest>, CreateEnvironmentRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
    }
}
