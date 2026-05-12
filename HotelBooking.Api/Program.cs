using System.Text.Json.Serialization;
using HotelBooking.Api.Data;
using HotelBooking.Api.Middleware;
using HotelBooking.Api.Seed;
using HotelBooking.Api.Services;
using HotelBooking.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Hotel Booking API",
        Version     = "v1",
        Description = "REST API for searching hotels, checking room availability, and making bookings."
    });

    // Tell Swagger how to render DateOnly as a date string
    options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type    = "string",
        Format  = "date",
        Example = new Microsoft.OpenApi.Any.OpenApiString("2026-06-01")
    });

    // Include XML comments from controller summaries
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Auto-apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Booking API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();

// Expose Program for integration test factory
public partial class Program { }
