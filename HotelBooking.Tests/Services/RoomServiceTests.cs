using HotelBooking.Api.Exceptions;
using HotelBooking.Api.Models;
using HotelBooking.Api.Models.Enums;
using HotelBooking.Api.Services;
using HotelBooking.Tests.Helpers;

namespace HotelBooking.Tests.Services;

public class RoomServiceTests
{
    private static async Task<(long hotelId, long doubleRoomId, long singleRoomId)> SeedHotelAsync(
        HotelBooking.Api.Data.AppDbContext context)
    {
        var hotel = new Hotel { Name = "Test Hotel" };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();

        var singleRoom = new Room { HotelId = hotel.Id, Type = RoomType.Single, Capacity = 1 };
        var doubleRoom = new Room { HotelId = hotel.Id, Type = RoomType.Double, Capacity = 2 };
        var deluxeRoom = new Room { HotelId = hotel.Id, Type = RoomType.Deluxe, Capacity = 4 };
        context.Rooms.AddRange(singleRoom, doubleRoom, deluxeRoom);
        await context.SaveChangesAsync();

        return (hotel.Id, doubleRoom.Id, singleRoom.Id);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_NoBookings_ReturnsAllRoomsWithSufficientCapacity()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, _, _) = await SeedHotelAsync(ctx);
        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 5),
            guestCount: 2);

        // Should return Double (cap 2) and Deluxe (cap 4), not Single (cap 1)
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, r => r.RoomType == "Single");
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_FullyOverlappingBooking_ExcludesRoom()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, doubleRoomId, _) = await SeedHotelAsync(ctx);
        ctx.Bookings.Add(new Booking
        {
            BookingReference = "BKTEST1",
            CustomerName     = "Test Guest",
            CustomerEmail    = "test@example.com",
            RoomId           = doubleRoomId,
            CheckInDate      = new DateOnly(2026, 7, 1),
            CheckOutDate     = new DateOnly(2026, 7, 5),
            GuestCount       = 2
        });
        await ctx.SaveChangesAsync();

        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 5),
            guestCount: 1);

        Assert.DoesNotContain(result, r => r.RoomId == doubleRoomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_PartialOverlapLeft_ExcludesRoom()
    {
        // Existing booking: Jun 1–5. Requested: Jun 3–7. Overlap on Jun 3–4.
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, doubleRoomId, _) = await SeedHotelAsync(ctx);
        ctx.Bookings.Add(new Booking
        {
            BookingReference = "BKTEST2",
            CustomerName     = "Test Guest",
            CustomerEmail    = "test@example.com",
            RoomId           = doubleRoomId,
            CheckInDate      = new DateOnly(2026, 6, 1),
            CheckOutDate     = new DateOnly(2026, 6, 5),
            GuestCount       = 2
        });
        await ctx.SaveChangesAsync();

        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 6, 3),
            new DateOnly(2026, 6, 7),
            guestCount: 1);

        Assert.DoesNotContain(result, r => r.RoomId == doubleRoomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_PartialOverlapRight_ExcludesRoom()
    {
        // Existing booking: Jun 5–10. Requested: Jun 3–7. Overlap on Jun 5–6.
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, doubleRoomId, _) = await SeedHotelAsync(ctx);
        ctx.Bookings.Add(new Booking
        {
            BookingReference = "BKTEST3",
            CustomerName     = "Test Guest",
            CustomerEmail    = "test@example.com",
            RoomId           = doubleRoomId,
            CheckInDate      = new DateOnly(2026, 6, 5),
            CheckOutDate     = new DateOnly(2026, 6, 10),
            GuestCount       = 2
        });
        await ctx.SaveChangesAsync();

        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 6, 3),
            new DateOnly(2026, 6, 7),
            guestCount: 1);

        Assert.DoesNotContain(result, r => r.RoomId == doubleRoomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_AdjacentCheckoutEqualsNewCheckin_IncludesRoom()
    {
        // Existing booking ends Jun 5 (departure day). Requested starts Jun 5. No overlap — room is free.
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, doubleRoomId, _) = await SeedHotelAsync(ctx);
        ctx.Bookings.Add(new Booking
        {
            BookingReference = "BKTEST4",
            CustomerName     = "Test Guest",
            CustomerEmail    = "test@example.com",
            RoomId           = doubleRoomId,
            CheckInDate      = new DateOnly(2026, 6, 1),
            CheckOutDate     = new DateOnly(2026, 6, 5),
            GuestCount       = 2
        });
        await ctx.SaveChangesAsync();

        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 6, 5),
            new DateOnly(2026, 6, 9),
            guestCount: 1);

        Assert.Contains(result, r => r.RoomId == doubleRoomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_AdjacentCheckinEqualsNewCheckout_IncludesRoom()
    {
        // Existing booking starts Jun 9. Requested ends Jun 9 (departure). No overlap.
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, doubleRoomId, _) = await SeedHotelAsync(ctx);
        ctx.Bookings.Add(new Booking
        {
            BookingReference = "BKTEST5",
            CustomerName     = "Test Guest",
            CustomerEmail    = "test@example.com",
            RoomId           = doubleRoomId,
            CheckInDate      = new DateOnly(2026, 6, 9),
            CheckOutDate     = new DateOnly(2026, 6, 15),
            GuestCount       = 2
        });
        await ctx.SaveChangesAsync();

        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 6, 5),
            new DateOnly(2026, 6, 9),
            guestCount: 1);

        Assert.Contains(result, r => r.RoomId == doubleRoomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_GuestCountExceedsCapacity_ExcludesRoom()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, _, singleRoomId) = await SeedHotelAsync(ctx);
        var service = new RoomService(ctx);

        var result = await service.GetAvailableRoomsAsync(
            hotelId,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 5),
            guestCount: 2); // Single only holds 1

        Assert.DoesNotContain(result, r => r.RoomId == singleRoomId);
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_CheckoutBeforeCheckin_ThrowsValidationException()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (hotelId, _, _) = await SeedHotelAsync(ctx);
        var service = new RoomService(ctx);

        await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.GetAvailableRoomsAsync(
                hotelId,
                new DateOnly(2026, 7, 5),
                new DateOnly(2026, 7, 1),
                guestCount: 1));
    }

    [Fact]
    public async Task GetAvailableRoomsAsync_HotelNotFound_ThrowsNotFoundException()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var service = new RoomService(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetAvailableRoomsAsync(
                999,
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 5),
                guestCount: 1));
    }
}
