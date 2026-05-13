using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.DTOs.Requests;

public class CreateBookingRequest
{
    [Required]
    public long RoomId { get; set; }

    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MinLength(1), MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public DateOnly CheckInDate { get; set; }

    [Required]
    public DateOnly CheckOutDate { get; set; }

    // Note: this max value hasn't been specified anywhere in the brief - I've picked
    // that feels like a reasonable limit for most rooms.
    [Required, Range(1, 8)]
    public int GuestCount { get; set; }
}
