using HotelBooking.Api.Models.Enums;

namespace HotelBooking.Api.Models;

public class Room
{
    public long Id { get; set; }
    
    public long HotelId { get; set; }
    public Hotel Hotel { get; set; } = null!;
    
    public RoomType Type { get; set; }
    
    public int Capacity { get; set; }
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
