using System.Net;

namespace FlagForge.Data.Exceptions;

public abstract class AppException(
    string errorCode,
    string message,
    string userMessage,
    HttpStatusCode statusCode,
    Dictionary<string, object>? details = null,
    bool isTransient = false)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
    public string UserMessage { get; } = userMessage;
    public Dictionary<string, object>? Details { get; } = details;
    public bool IsTransient { get; } = isTransient;
}