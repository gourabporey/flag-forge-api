using FlagForge.Data.Constants;

namespace FlagForge.Data.Exceptions;

using System.Net;

public sealed class TransientServerException(
    string message = "A temporary server error occurred. Please try again.",
    Dictionary<string, object>? details = null
)
    : AppException(
        errorCode: ErrorCodes.TransientFailure,
        message: message,
        userMessage: message,
        statusCode: HttpStatusCode.ServiceUnavailable,
        details: details,
        isTransient: true
    );
