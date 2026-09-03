using System.Net;
using System.Text.Json;
using ProductManagement.API.Models;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");

            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType =
            "application/json";

        var response = exception switch
        {
            NotFoundException => new ErrorResponse
            {
                StatusCode =
                    (int)HttpStatusCode.NotFound,

                Message = exception.Message
            },

            BadRequestException => new ErrorResponse
            {
                StatusCode =
                    (int)HttpStatusCode.BadRequest,

                Message = exception.Message
            },

            UnauthorizedException => new ErrorResponse
            {
                StatusCode =
                    (int)HttpStatusCode.Unauthorized,

                Message = exception.Message
            },

            _ => new ErrorResponse
            {
                StatusCode =
                    (int)HttpStatusCode.InternalServerError,

                Message =
                    "An unexpected error occurred."
            }
        };

        context.Response.StatusCode =
            response.StatusCode;

        var jsonResponse =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(
            jsonResponse);
    }
}