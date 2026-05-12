using HotelBooking.Api.Data;
using HotelBooking.Api.DTOs.Responses;
using HotelBooking.Api.Exceptions;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class RoomService(AppDbContext context) : IRoomService
{
    public async Task<IEnumerable<AvailableRoomResponse>> GetAvailableRoomsAsync(
        long hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestCount)
    {
        if (checkOut <= checkIn)
            throw new BookingValidationException("Check-out date must be after check-in date.");

        if (guestCount < 1)
            throw new BookingValidationException("Guest count must be at least 1.");

        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
            throw new NotFoundException($"Hotel with ID {hotelId} was not found.");

        // Half-open interval overlap: existing.CheckIn < requested.CheckOut AND existing.CheckOut > requested.CheckIn
        var availableRooms = await context.Rooms
            .Where(r => r.HotelId == hotelId)
            .Where(r => r.Capacity >= guestCount)
            .Where(r => !r.Bookings.Any(b =>
                b.CheckInDate < checkOut &&
                b.CheckOutDate > checkIn))
            .Include(r => r.Hotel)
            .Select(r => new AvailableRoomResponse
            {
                RoomId    = r.Id,
                RoomType  = r.Type.ToString(),
                Capacity  = r.Capacity,
                HotelId   = r.HotelId,
                HotelName = r.Hotel.Name
            })
            .ToListAsync();

        return availableRooms;
    }
}
