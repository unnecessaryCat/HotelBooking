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
    /// <summary>List all hotels, optionally filtered by a partial name match.</summary>
    /// <param name="name">Optional partial name to filter by.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HotelResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHotels([FromQuery] string? name)
    {
        var hotels = await hotelService.GetHotelsAsync(name);
        return Ok(hotels);
    }

    /// <summary>Find a single hotel by exact name.</summary>
    /// <param name="name">The exact hotel name to look up.</param>
    [HttpGet("find")]
    [ProducesResponseType(typeof(HotelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //There is a bit of an issue with this - name isn't a unique field. We could make it unique
    //but that doesn't feel correct. Generally, I would prefer to either have a business 
    //limitation stating all names must be unique or use the id as the input parameter.
    public async Task<IActionResult> FindByExactName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "The 'name' query parameter is required." });

        var hotel = await hotelService.FindByExactNameAsync(name);

        return hotel is null
            ? NotFound(new { error = $"No hotel found with the name '{name}'." })
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
