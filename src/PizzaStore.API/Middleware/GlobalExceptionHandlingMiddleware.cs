using PizzaStore.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace PizzaStore.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = "An error occurred processing your request",
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = validationEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = notFoundEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = new
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = unauthorizedEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse = new
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = forbiddenEx.Message,
                    Timestamp = DateTime.UtcNow
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An internal server error occurred",
                    Timestamp = DateTime.UtcNow
                };
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }
}
