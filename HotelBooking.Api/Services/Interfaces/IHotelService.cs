using HotelBooking.Api.DTOs.Responses;

namespace HotelBooking.Api.Services.Interfaces;

public interface IHotelService
{
    Task<List<HotelResponse>> GetHotelsAsync(string? name);
    Task<HotelResponse?> FindByExactNameAsync(string name);
}
