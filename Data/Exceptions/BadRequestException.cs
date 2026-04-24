using System.Net;
using FlagForge.Data.Constants;

namespace FlagForge.Data.Exceptions;

public sealed class BadRequestException(string message, Dictionary<string, object>? details = null)
    : AppException(ErrorCodes.ValidationFailed, message, message, HttpStatusCode.BadRequest, details);
