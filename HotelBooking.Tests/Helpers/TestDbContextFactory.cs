using HotelBooking.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HotelBooking.Tests.Helpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an isolated in-memory DbContext for unit testing.
    /// Transactions are not supported by the in-memory store so the warning is suppressed —
    /// the transaction begin/commit/rollback calls are ignored (no-op) instead of throwing.
    /// </summary>
    public static AppDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}
