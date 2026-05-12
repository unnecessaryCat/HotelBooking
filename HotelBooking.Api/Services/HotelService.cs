using HotelBooking.Api.Data;
using HotelBooking.Api.DTOs.Responses;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class HotelService(AppDbContext context) : IHotelService
{
    public async Task<HotelResponse?> FindByNameAsync(string name)
    {
        var hotel = await context.Hotels
            .Where(h => EF.Functions.Like(h.Name, $"%{name}%"))
            .Select(h => new HotelResponse { Id = h.Id, Name = h.Name })
            .FirstOrDefaultAsync();

        return hotel;
    }
}
