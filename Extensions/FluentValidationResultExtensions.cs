using FluentValidation;
using FluentValidation.Results;

namespace FlagForge.Extensions;

public static class FluentValidationResultExtensions
{
    public static IDictionary<string, string> ToErrorsDictionary(this ValidationResult validationResult)
    {
        var errDict = new Dictionary<string, string>();
        foreach (var failure in validationResult.Errors)
        {
            errDict[failure.PropertyName] = failure.ErrorMessage;
        }

        return errDict;
    }
}