using HotelBooking.Api.DTOs.Responses;

namespace HotelBooking.Api.Services.Interfaces;

public interface IHotelService
{
    Task<HotelResponse?> FindByNameAsync(string name);
}
