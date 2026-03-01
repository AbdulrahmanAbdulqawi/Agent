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
                    Errors = validationEx.Errors.Select(e => new ValidationError
                    {
                        Property = e.PropertyName,
                        Message = e.ErrorMessage
                    }).ToList()
                }
            ),
            InvalidOperationException invalidOp => (
                HttpStatusCode.BadRequest,
                new ErrorResponse { Type = "InvalidOperation", Message = invalidOp.Message }
            ),
            HttpRequestException httpEx => (
                HttpStatusCode.BadGateway,
                new ErrorResponse { Type = "ExternalServiceError", Message = "An error occurred while communicating with an external service" }
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse { Type = "ArgumentError", Message = argEx.Message }
            ),
            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse { Type = "NotFound", Message = notFoundEx.Message }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse { Type = "InternalError", Message = "An unexpected error occurred" }
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
    public List<ValidationError>? Errors { get; set; }
    public string? TraceId { get; set; }
}

public class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
