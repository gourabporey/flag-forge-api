using FlagForge.Data.Constants;

namespace FlagForge.Data.Exceptions;

using System.Net;

public sealed class InternalServerException(
    string message = "An unexpected server error occurred.",
    Dictionary<string, object>? details = null
)
    : AppException(
        errorCode: ErrorCodes.InternalError,
        message: message,
        userMessage: "Something went wrong. Please contact support if the issue persists.",
        statusCode: HttpStatusCode.InternalServerError,
        details: details,
        isTransient: false
    );
