using System.Net;
using System.Text.Json;
using UserService.Exceptions;
using UserService.Models;

namespace UserService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var headerCorrelationId))
        {
            correlationId = headerCorrelationId.ToString();
        }
        
        var problemDetails = CreateProblemDetails(exception, context.Request.Path, correlationId);
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problemDetails.Status;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);

        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, string instance, string correlationId)
    {
        return exception switch
        {
            EmailAlreadyExistsException => new ProblemDetails
            {
                Title = "VALIDATION_ERROR",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "Unable to create your account. Try logging in or resetting your password if an account exists with this email.",
                Instance = instance,
                CorrelationId = correlationId
            },
            UnauthorizedTokenException => new ProblemDetails
            {
                Title = "UNAUTHORIZED",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "You are unauthroized to access this endpoint, please create or log in to your account.",
                Instance = instance,
                CorrelationId = correlationId
            },
            LoginFailedException => new ProblemDetails
            {
                Title = "AUTHENTICATION_FAILED",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "Invalid email or password",
                Instance = instance,
                CorrelationId = correlationId
            },
            SuspendedUserException => new ProblemDetails
            {
                Title = "USER_SUSPENDED",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "The requested user has been suspended",
                Instance = instance,
                CorrelationId = correlationId
            },
            NotFoundException => new ProblemDetails
            {
                Title = "NOT_FOUND",
                Status = (int)HttpStatusCode.NotFound,
                Detail = "User not found",
                Instance = instance,
                CorrelationId = correlationId
            },
            _ => new ProblemDetails
            {
                Title = "An error occurred while processing your request",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = instance,
                CorrelationId = correlationId
            }
        };
    }
}