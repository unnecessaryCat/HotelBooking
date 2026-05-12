using HotelBooking.Api.DTOs.Requests;
using HotelBooking.Api.DTOs.Responses;

namespace HotelBooking.Api.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
    Task<BookingResponse?> GetByReferenceAsync(string reference);
}
