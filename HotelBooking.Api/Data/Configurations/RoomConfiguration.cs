using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Api.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.Capacity)
            .IsRequired();

        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
