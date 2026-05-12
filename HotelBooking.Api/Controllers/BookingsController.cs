using HotelBooking.Api.DTOs.Requests;
using HotelBooking.Api.DTOs.Responses;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("bookings")]
[Produces("application/json")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    /// <summary>Create a new room booking.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var booking = await bookingService.CreateBookingAsync(request);
        return CreatedAtAction(
            nameof(GetByReference),
            new { reference = booking.BookingReference },
            booking);
    }

    /// <summary>Retrieve booking details by booking reference number.</summary>
    /// <param name="reference">The unique booking reference (e.g. BKXR4T9A).</param>
    [HttpGet("{reference}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string reference)
    {
        var booking = await bookingService.GetByReferenceAsync(reference);

        return booking is null
            ? NotFound(new { error = $"No booking found with reference '{reference}'." })
            : Ok(booking);
    }
}
