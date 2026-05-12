using HotelBooking.Api.DTOs.Responses;
using HotelBooking.Api.Exceptions;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("hotels")]
[Produces("application/json")]
public class HotelsController(IHotelService hotelService, IRoomService roomService) : ControllerBase
{
    /// <summary>Find a hotel by name (partial match, case-insensitive).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "The 'name' query parameter is required." });

        var hotel = await hotelService.FindByNameAsync(name);

        return hotel is null
            ? NotFound(new { error = $"No hotel found matching '{name}'." })
            : Ok(hotel);
    }

    /// <summary>Find available rooms in a hotel between two dates for a given number of guests.</summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <param name="checkIn">Check-in date (yyyy-MM-dd).</param>
    /// <param name="checkOut">Check-out / departure date (yyyy-MM-dd). Must be after check-in.</param>
    /// <param name="guests">Number of guests.</param>
    [HttpGet("{hotelId}/rooms/available")]
    [ProducesResponseType(typeof(IEnumerable<AvailableRoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableRooms(
        long hotelId,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int guests)
    {
        var rooms = await roomService.GetAvailableRoomsAsync(hotelId, checkIn, checkOut, guests);
        return Ok(rooms);
    }
}
