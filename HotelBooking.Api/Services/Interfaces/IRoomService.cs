using HotelBooking.Api.DTOs.Responses;

namespace HotelBooking.Api.Services.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<AvailableRoomResponse>> GetAvailableRoomsAsync(
        long hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        int guestCount);
}
