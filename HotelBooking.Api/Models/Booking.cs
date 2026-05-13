using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.Models;

public class Booking
{
    public long Id { get; set; }
    
    public string BookingReference { get; set; } = string.Empty;
    
    public string CustomerName { get; set; } = string.Empty;
    
    public string CustomerEmail { get; set; } = string.Empty;
    
    public DateOnly CheckInDate { get; set; }
    
    public DateOnly CheckOutDate { get; set; }
    
    public int GuestCount { get; set; }
    
    public long RoomId { get; set; }
    
    public Room Room { get; set; } = null!;
}
