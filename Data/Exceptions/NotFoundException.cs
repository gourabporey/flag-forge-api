using System.Net;
using FlagForge.Data.Constants;

namespace FlagForge.Data.Exceptions;

public sealed class NotFoundException(string resourceName, object key, bool isTransient)
    : AppException(
        ErrorCodes.ResourceNotFound,
        $"{resourceName} with identifier '{key}' was not found.",
        $"{resourceName} with identifier '{key}' was not found.",
        HttpStatusCode.NotFound
    );
