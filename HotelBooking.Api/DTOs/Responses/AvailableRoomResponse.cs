namespace HotelBooking.Api.DTOs.Responses;

public class AvailableRoomResponse
{
    public long RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public long HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
}
