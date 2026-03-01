using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Agent.Api.Middleware;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Type = "ValidationError",
                    Message = "One or more validation errors occurred",
                    Errors = validationEx.Errors
                        .Select(e => new ErrorDetail { Field = e.PropertyName, Message = e.ErrorMessage })
                        .ToList()
                }
            ),
            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Type = "InvalidOperation",
                    Message = invalidOpEx.Message
                }
            ),
            HttpRequestException httpEx => (
                HttpStatusCode.BadGateway,
                new ErrorResponse
                {
                    Type = "ExternalServiceError",
                    Message = "Failed to communicate with external service",
                    Errors = [new ErrorDetail { Message = httpEx.Message }]
                }
            ),
            TaskCanceledException => (
                HttpStatusCode.RequestTimeout,
                new ErrorResponse
                {
                    Type = "Timeout",
                    Message = "The request timed out"
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalError",
                    Message = "An unexpected error occurred"
                }
            )
        };

        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public List<ErrorDetail>? Errors { get; set; }
}

public class ErrorDetail
{
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;
}
