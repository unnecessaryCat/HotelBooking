using HotelBooking.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace HotelBooking.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException ex           => (StatusCodes.Status404NotFound, ex.Message),
            ConflictException ex           => (StatusCodes.Status409Conflict, ex.Message),
            BookingValidationException ex  => (StatusCodes.Status400BadRequest, ex.Message),
            _                              => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message }, cancellationToken);

        return true;
    }
}
