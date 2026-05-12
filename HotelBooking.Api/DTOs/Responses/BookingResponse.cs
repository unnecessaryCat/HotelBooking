namespace HotelBooking.Api.DTOs.Responses;

public class BookingResponse
{
    public long Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int GuestCount { get; set; }
}
