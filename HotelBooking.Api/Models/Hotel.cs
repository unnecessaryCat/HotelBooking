namespace HotelBooking.Api.Models;

public class Hotel
{
    public long Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
