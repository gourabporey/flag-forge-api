using System.Net;

namespace FlagForge.Data.Exceptions;

public sealed class ConflictException(string errorCode, string message, string userMessage)
    : AppException(errorCode, message, userMessage, HttpStatusCode.Conflict);
