using System.Net;
using FlagForge.Data.Constants;

namespace FlagForge.Data.Exceptions;

public sealed class RequestValidationException(IDictionary<string, string> errors)
    : AppException(
        ErrorCodes.ValidationFailed,
        "Validation failed",
        "One or more validation errors occurred.",
        HttpStatusCode.BadRequest
    )
{
    public IDictionary<string, string> Errors { get; } = errors;
}
