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

    [Required, Range(1, 20)]
    public int GuestCount { get; set; }
}
