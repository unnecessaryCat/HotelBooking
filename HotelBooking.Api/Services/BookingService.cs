using System.Data;
using HotelBooking.Api.Data;
using HotelBooking.Api.DTOs.Requests;
using HotelBooking.Api.DTOs.Responses;
using HotelBooking.Api.Exceptions;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class BookingService(AppDbContext context) : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
    {
        // Trim whitespace and convert to lowercase - to ensure uniqueness.
        // We could check if the email is valid if it isn't, we could throw an error or warning but out of scope for now.
        request.CustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant();

        if (request.CheckOutDate <= request.CheckInDate)
            throw new BookingValidationException("Check-out date must be after check-in date.");

        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.RepeatableRead);

        try
        {
            var room = await context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId)
                ?? throw new NotFoundException($"Room with ID {request.RoomId} was not found.");

            if (request.GuestCount > room.Capacity)
                throw new BookingValidationException(
                    $"Requested {request.GuestCount} guests exceed room capacity of {room.Capacity}.");

            // Check to make sure a conflicting booking doesn't already exist inside
            // the transaction to guard against race conditions.
            var bookingConflict = await context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.CheckInDate <= request.CheckOutDate &&
                b.CheckOutDate > request.CheckInDate);

            if (bookingConflict)
                throw new ConflictException(
                    "The room is not available for the requested dates.");

            var reference = Guid.NewGuid().ToString();

            var booking = new Booking
            {
                BookingReference = reference,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                GuestCount = request.GuestCount
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(booking, room);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingResponse?> GetByReferenceAsync(string reference)
    {
        var booking = await context.Bookings
            .Include(b => b.Room)
            .ThenInclude(r => r.Hotel)
            .FirstOrDefaultAsync(b => b.BookingReference == reference.ToUpperInvariant());

        return booking is null ? null : MapToResponse(booking, booking.Room);
    }

    // Could use something like AutoMap, if it was bigger, this is manual but ok for now
    private static BookingResponse MapToResponse(Booking booking, Room room) => new()
    {
        Id = booking.Id,
        BookingReference = booking.BookingReference,
        CustomerName = booking.CustomerName,
        CustomerEmail = booking.CustomerEmail,
        RoomId = booking.RoomId,
        RoomType = room.Type.ToString(),
        HotelName = room.Hotel.Name,
        CheckInDate = booking.CheckInDate,
        CheckOutDate = booking.CheckOutDate,
        GuestCount = booking.GuestCount
    };
}
