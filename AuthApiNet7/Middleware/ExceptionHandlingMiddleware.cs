using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudioMedico.AuthApi.Services.Auth;

namespace StudioMedico.AuthApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed: {Path}", context.Request.Path);
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = MapException(exception);

        context.Response.StatusCode = statusCode;

        var body = new ErrorResponse
        {
            Message = message,
            Status = statusCode
        };

        await context.Response.WriteAsJsonAsync(body);
    }

    private (int StatusCode, string Message) MapException(Exception exception)
    {
        switch (exception)
        {
            case EmailAlreadyExistsException e:
                return (StatusCodes.Status409Conflict, e.Message);
            case InvalidCredentialsException e:
                return (StatusCodes.Status401Unauthorized, e.Message);
            case InvalidRoleException e:
                return (StatusCodes.Status400BadRequest, e.Message);
            case DbUpdateException dbEx when IsPostgresUniqueViolation(dbEx):
                return (StatusCodes.Status409Conflict, "User with this email already exists.");
            default:
                if (_env.IsDevelopment())
                    return (StatusCodes.Status500InternalServerError, exception.Message);

                return (StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static bool IsPostgresUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }
}

internal static class PostgresErrorCodes
{
    public const string UniqueViolation = "23505";
}

public sealed class ErrorResponse
{
    public string Message { get; init; } = string.Empty;
    public int Status { get; init; }
}
