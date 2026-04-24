using System.Diagnostics;
using FlagForge.Data.Constants;
using FlagForge.Data.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FlagForge.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct
    )
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        LogException(exception, traceId);

        var (statusCode, title, type) = MapException(exception);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Instance = context.Request.Path,
            Detail = GetSafeMessage(exception),
            Extensions =
            {
                ["errorCode"] = GetErrorCode(exception),
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow,
            },
        };

        if (exception is RequestValidationException validationEx)
        {
            problem.Extensions["errors"] = validationEx.Errors;
        }

        if (exception is AppException appEx)
        {
            if (appEx.Details is not null)
                problem.Extensions["details"] = appEx.Details;

            problem.Extensions["retryable"] = appEx.IsTransient;

            if (appEx.IsTransient)
            {
                context.Response.Headers.RetryAfter = "5";
            }
        }

        context.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem }
        );
    }

    private static (int StatusCode, string Title, string Type) MapException(Exception ex) =>
        ex switch
        {
            AppException appEx => (
                (int)appEx.StatusCode,
                GetTitle(appEx),
                GetTypeUri((int)appEx.StatusCode)
            ),

            ArgumentNullException => (400, "Invalid argument provided", GetTypeUri(400)),
            ArgumentException => (400, "Invalid argument provided", GetTypeUri(400)),
            UnauthorizedAccessException => (401, "Unauthorized", GetTypeUri(401)),

            _ => (500, "Internal Server Error", GetTypeUri(500)),
        };

    private static string GetTitle(AppException ex) =>
        ex switch
        {
            RequestValidationException => "Validation failed",
            NotFoundException => "Resource not found",
            ConflictException => "Conflict occurred",
            BadRequestException => "Bad request",
            _ => "Application error",
        };

    private static string GetTypeUri(int statusCode) =>
        $"https://api.flag-forge-api.onrender.com/errors/{statusCode}";

    private static string GetErrorCode(Exception ex) =>
        ex switch
        {
            AppException appEx => appEx.ErrorCode,
            ArgumentException => ErrorCodes.ValidationFailed,
            UnauthorizedAccessException => ErrorCodes.Unauthorized,
            _ => ErrorCodes.InternalError,
        };

    private static string GetSafeMessage(Exception ex) =>
        ex switch
        {
            AppException appEx => appEx.UserMessage,
            _ => "An unexpected error occurred.",
        };

    private void LogException(Exception ex, string traceId)
    {
        switch (ex)
        {
            case RequestValidationException:
            case BadRequestException:
                logger.LogWarning(ex, "Client error. TraceId: {TraceId}", traceId);
                break;

            case ConflictException:
            case NotFoundException:
                logger.LogInformation(ex, "Business rule triggered. TraceId: {TraceId}", traceId);
                break;

            case AppException:
                logger.LogWarning(ex, "Application exception. TraceId: {TraceId}", traceId);
                break;

            default:
                logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
                break;
        }
    }
}
