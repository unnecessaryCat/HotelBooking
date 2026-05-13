using HotelBooking.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Api.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingReference)
            .IsRequired()
            .HasMaxLength(40);

        builder.HasIndex(b => b.BookingReference)
            .IsUnique();

        builder.Property(b => b.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.CustomerEmail)
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("Latin1_General_CI_AS");

        builder.Property(b => b.GuestCount)
            .IsRequired();

        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
