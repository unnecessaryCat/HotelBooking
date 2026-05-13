using HotelBooking.Api.Data;
using HotelBooking.Api.DTOs.Responses;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class HotelService(AppDbContext context) : IHotelService
{
    public async Task<List<HotelResponse>> GetHotelsAsync(string? name)
    {
        var query = context.Hotels.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(h => EF.Functions.Like(h.Name, $"%{name.Trim()}%"));

        return await query
            .Select(h => new HotelResponse { Id = h.Id, Name = h.Name })
            .ToListAsync();
    }

    public async Task<HotelResponse?> FindByExactNameAsync(string name)
    {
        var normalised = name.Trim();

        return await context.Hotels
            .Where(h => h.Name == normalised)
            .Select(h => new HotelResponse { Id = h.Id, Name = h.Name })
            .FirstOrDefaultAsync();
    }
}
