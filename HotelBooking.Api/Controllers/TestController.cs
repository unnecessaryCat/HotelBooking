using HotelBooking.Api.Seed;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.Api.Controllers;

/// <summary>
/// Test utilities for seeding and resetting data.
/// These endpoints exist purely to support manual and automated testing.
/// </summary>
[ApiController]
[Route("test")]
[Produces("application/json")]
public class TestController(DatabaseSeeder seeder) : ControllerBase
{
    /// <summary>
    /// Populate the database with test data (hotels, rooms and sample bookings).
    /// This operation is idempotent — calling it multiple times has no additional effect.
    /// </summary>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed()
    {
        await seeder.SeedAsync();
        return Ok(new { message = "Database seeded successfully." });
    }

    /// <summary>
    /// Remove all data from the database, ready for a fresh seed.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reset()
    {
        await seeder.ResetAsync();
        return NoContent();
    }
}
