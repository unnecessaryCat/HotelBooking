using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Seed;

public class DatabaseSeeder(AppDbContext context)
{
    public async Task SeedAsync()
    {
        // Idempotent — skip if hotels already exist
        if (await context.Hotels.AnyAsync())
            return;

        var hotel1 = new Hotel { Name = "The Grand Pelican" };
        var hotel2 = new Hotel { Name = "Harbour View Suites" };

        context.Hotels.AddRange(hotel1, hotel2);
        await context.SaveChangesAsync();

        // 6 rooms per hotel: 2x Single, 2x Double, 2x Deluxe
        var rooms = new List<Room>
        {
            new() { HotelId = hotel1.Id, Type = RoomType.Single, Capacity = 1 },
            new() { HotelId = hotel1.Id, Type = RoomType.Single, Capacity = 1 },
            new() { HotelId = hotel1.Id, Type = RoomType.Double, Capacity = 2 },
            new() { HotelId = hotel1.Id, Type = RoomType.Double, Capacity = 2 },
            new() { HotelId = hotel1.Id, Type = RoomType.Deluxe, Capacity = 4 },
            new() { HotelId = hotel1.Id, Type = RoomType.Deluxe, Capacity = 4 },

            new() { HotelId = hotel2.Id, Type = RoomType.Single, Capacity = 1 },
            new() { HotelId = hotel2.Id, Type = RoomType.Single, Capacity = 1 },
            new() { HotelId = hotel2.Id, Type = RoomType.Double, Capacity = 2 },
            new() { HotelId = hotel2.Id, Type = RoomType.Double, Capacity = 2 },
            new() { HotelId = hotel2.Id, Type = RoomType.Deluxe, Capacity = 4 },
            new() { HotelId = hotel2.Id, Type = RoomType.Deluxe, Capacity = 4 }
        };

        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();

        // Sample bookings to demonstrate occupied rooms for testing
        var hotel1DoubleRoom = rooms.First(r => r.HotelId == hotel1.Id && r.Type == RoomType.Double);
        var hotel2DeluxeRoom = rooms.First(r => r.HotelId == hotel2.Id && r.Type == RoomType.Deluxe);

        var bookings = new List<Booking>
        {
            new()
            {
                BookingReference = "BKSEED01",
                CustomerName     = "Alice Johnson",
                CustomerEmail    = "alice.johnson@example.com",
                RoomId           = hotel1DoubleRoom.Id,
                CheckInDate      = new DateOnly(2026, 6, 1),
                CheckOutDate     = new DateOnly(2026, 6, 5),
                GuestCount       = 2
            },
            new()
            {
                BookingReference = "BKSEED02",
                CustomerName     = "Bob Smith",
                CustomerEmail    = "bob.smith@example.com",
                RoomId           = hotel2DeluxeRoom.Id,
                CheckInDate      = new DateOnly(2026, 6, 10),
                CheckOutDate     = new DateOnly(2026, 6, 12),
                GuestCount       = 3
            }
        };

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
    }

    public async Task ResetAsync()
    {
        context.Bookings.RemoveRange(context.Bookings);
        context.Rooms.RemoveRange(context.Rooms);
        context.Hotels.RemoveRange(context.Hotels);
        await context.SaveChangesAsync();
    }
}
