using HotelBooking.Api.DTOs.Requests;
using HotelBooking.Api.Exceptions;
using HotelBooking.Api.Models;
using HotelBooking.Api.Models.Enums;
using HotelBooking.Api.Services;
using HotelBooking.Tests.Helpers;

namespace HotelBooking.Tests.Services;

public class BookingServiceTests
{
    private static async Task<(long hotelId, long doubleRoomId)> SeedRoomAsync(
        HotelBooking.Api.Data.AppDbContext context,
        RoomType type = RoomType.Double,
        int capacity = 2)
    {
        var hotel = new Hotel { Name = "Test Hotel" };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();

        var room = new Room { HotelId = hotel.Id, Type = type, Capacity = capacity };
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        return (hotel.Id, room.Id);
    }

    [Fact]
    public async Task CreateBookingAsync_ValidRequest_ReturnsBkPrefixedReference()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (_, roomId) = await SeedRoomAsync(ctx);
        var service = new BookingService(ctx);

        var result = await service.CreateBookingAsync(new CreateBookingRequest
        {
            RoomId        = roomId,
            CustomerName  = "Jane Doe",
            CustomerEmail = "jane@example.com",
            CheckInDate   = new DateOnly(2026, 8, 1),
            CheckOutDate  = new DateOnly(2026, 8, 5),
            GuestCount    = 2
        });

        Assert.StartsWith("BK", result.BookingReference);
        Assert.True(result.BookingReference.Length > 2); // BK + GUID
    }

    [Fact]
    public async Task CreateBookingAsync_NormalisesEmail()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (_, roomId) = await SeedRoomAsync(ctx);
        var service = new BookingService(ctx);

        var result = await service.CreateBookingAsync(new CreateBookingRequest
        {
            RoomId        = roomId,
            CustomerName  = "Jane Doe",
            CustomerEmail = "  Jane.DOE@Example.COM  ",
            CheckInDate   = new DateOnly(2026, 8, 1),
            CheckOutDate  = new DateOnly(2026, 8, 5),
            GuestCount    = 2
        });

        Assert.Equal("jane.doe@example.com", result.CustomerEmail);
    }

    [Fact]
    public async Task CreateBookingAsync_OverlappingDates_ThrowsConflictException()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (_, roomId) = await SeedRoomAsync(ctx);
        var service = new BookingService(ctx);

        // First booking
        await service.CreateBookingAsync(new CreateBookingRequest
        {
            RoomId        = roomId,
            CustomerName  = "Guest One",
            CustomerEmail = "guest1@example.com",
            CheckInDate   = new DateOnly(2026, 8, 1),
            CheckOutDate  = new DateOnly(2026, 8, 7),
            GuestCount    = 2
        });

        // Second booking overlapping the first
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                RoomId        = roomId,
                CustomerName  = "Guest Two",
                CustomerEmail = "guest2@example.com",
                CheckInDate   = new DateOnly(2026, 8, 5),
                CheckOutDate  = new DateOnly(2026, 8, 10),
                GuestCount    = 2
            }));
    }

    [Fact]
    public async Task CreateBookingAsync_GuestCountExceedsCapacity_ThrowsValidationException()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (_, roomId) = await SeedRoomAsync(ctx, RoomType.Single, capacity: 1);
        var service = new BookingService(ctx);

        await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                RoomId        = roomId,
                CustomerName  = "Crowded Guest",
                CustomerEmail = "crowded@example.com",
                CheckInDate   = new DateOnly(2026, 8, 1),
                CheckOutDate  = new DateOnly(2026, 8, 5),
                GuestCount    = 3
            }));
    }

    [Fact]
    public async Task CreateBookingAsync_RoomNotFound_ThrowsNotFoundException()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var service = new BookingService(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                RoomId        = 999,
                CustomerName  = "Nobody",
                CustomerEmail = "nobody@example.com",
                CheckInDate   = new DateOnly(2026, 8, 1),
                CheckOutDate  = new DateOnly(2026, 8, 5),
                GuestCount    = 1
            }));
    }

    [Fact]
    public async Task CreateBookingAsync_CheckoutNotAfterCheckin_ThrowsValidationException()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (_, roomId) = await SeedRoomAsync(ctx);
        var service = new BookingService(ctx);

        await Assert.ThrowsAsync<BookingValidationException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest
            {
                RoomId        = roomId,
                CustomerName  = "Bad Dates",
                CustomerEmail = "dates@example.com",
                CheckInDate   = new DateOnly(2026, 8, 5),
                CheckOutDate  = new DateOnly(2026, 8, 1),
                GuestCount    = 1
            }));
    }

    [Fact]
    public async Task GetByReferenceAsync_ExistingReference_ReturnsBooking()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var (_, roomId) = await SeedRoomAsync(ctx);
        var service = new BookingService(ctx);

        var created = await service.CreateBookingAsync(new CreateBookingRequest
        {
            RoomId        = roomId,
            CustomerName  = "Find Me",
            CustomerEmail = "findme@example.com",
            CheckInDate   = new DateOnly(2026, 9, 1),
            CheckOutDate  = new DateOnly(2026, 9, 3),
            GuestCount    = 1
        });

        var found = await service.GetByReferenceAsync(created.BookingReference);

        Assert.NotNull(found);
        Assert.Equal(created.BookingReference, found!.BookingReference);
        Assert.Equal("findme@example.com", found.CustomerEmail);
    }

    [Fact]
    public async Task GetByReferenceAsync_UnknownReference_ReturnsNull()
    {
        var ctx = TestDbContextFactory.CreateInMemory();
        var service = new BookingService(ctx);

        var result = await service.GetByReferenceAsync("BKXXXXXX");

        Assert.Null(result);
    }
}
