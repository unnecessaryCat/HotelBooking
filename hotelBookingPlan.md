# Hotel Room Booking API — Implementation Plan

## Context
Building a greenfield Hotel Room Booking REST API in ASP.NET Core 8 with EF Core and SQL Server.
Requirements: multi-hotel, 3 room types, 6 rooms per hotel, strict availability enforcement,
unique booking references, capacity enforcement, no authentication.

---

## Architecture Decisions

| Decision | Choice | Reason |
|---|---|---|
| Project layout | Single API project + test project | Appropriate scope, avoids over-engineering |
| API style | Controller-based | Better Swagger ergonomics |
| Repository layer | Omitted (DbContext direct) | EF Core IS the unit of work |
| Date type | `DateOnly` | Eliminates timezone bugs, maps to SQL `date` natively in EF 8 |
| Checkout semantics | Departure day (exclusive) | Industry standard half-open interval |
| Booking reference | `BK` + GUID | Globally unique, collision-free |
| Concurrency guard | `RepeatableRead` transaction on booking creation | Prevents double-booking race conditions |
| Enum storage | String in DB | Readable, migration-safe |
| Primary key type | `long` (SQL `bigint`) | Future-safe, larger ID space than `int` |
| Email storage | Normalised: `.Trim().ToLowerInvariant()` in service layer + `Latin1_General_CI_AS` collation in DB | Ensures `John@Email.com` and `john@email.com ` are treated identically |

---

## Solution Structure

```
HotelBooking/
├── HotelBooking.Api/
│   ├── Controllers/
│   │   ├── HotelsController.cs
│   │   ├── BookingsController.cs
│   │   └── TestController.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── HotelConfiguration.cs
│   │   │   ├── RoomConfiguration.cs
│   │   │   └── BookingConfiguration.cs
│   │   └── Migrations/
│   ├── Models/
│   │   ├── Hotel.cs
│   │   ├── Room.cs
│   │   ├── Booking.cs
│   │   └── Enums/RoomType.cs
│   ├── DTOs/
│   │   ├── Requests/CreateBookingRequest.cs
│   │   └── Responses/
│   │       ├── HotelResponse.cs
│   │       ├── AvailableRoomResponse.cs
│   │       └── BookingResponse.cs
│   ├── Services/
│   │   ├── Interfaces/ (IHotelService, IRoomService, IBookingService)
│   │   ├── HotelService.cs
│   │   ├── RoomService.cs
│   │   └── BookingService.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── ConflictException.cs
│   │   └── BookingValidationException.cs
│   ├── Middleware/GlobalExceptionHandler.cs
│   ├── Seed/DatabaseSeeder.cs
│   ├── Program.cs
│   └── appsettings.json
├── HotelBooking.Tests/
│   ├── Services/
│   │   ├── RoomServiceTests.cs
│   │   └── BookingServiceTests.cs
│   └── Helpers/TestDbContextFactory.cs
├── docker-compose.yml
├── .env                  (gitignored)
├── .gitignore
└── HotelBooking.slnx
```

---

## Entities

**RoomType** (enum): `Single=1, Double=2, Deluxe=3`

**Hotel**: `Id` (`long`), `Name` (indexed for name search), nav: `Rooms`

**Room**: `Id` (`long`), `HotelId` (`long` FK), `Type` (RoomType), `Capacity` (Single=1, Double=2, Deluxe=4), nav: `Hotel`, `Bookings`

**Booking**: `Id` (`long`), `BookingReference` (unique index), `CustomerName`, `CustomerEmail` (required, non-empty, normalised to lowercase+trimmed, `CI_AS` collation), `RoomId` (`long` FK), `CheckInDate` (DateOnly), `CheckOutDate` (DateOnly), `GuestCount`

---

## API Endpoints

| Method | Route | Description | Response |
|---|---|---|---|
| `GET` | `/hotels?name={name}` | Find hotel by name | 200 HotelResponse / 404 |
| `GET` | `/hotels/{hotelId}/rooms/available?checkIn=&checkOut=&guests=` | Available rooms for dates + guests | 200 AvailableRoomResponse[] |
| `POST` | `/bookings` | Create booking | 201 BookingResponse / 400 / 409 |
| `GET` | `/bookings/{reference}` | Get booking by reference | 200 BookingResponse / 404 |
| `POST` | `/test/seed` | Seed test data (idempotent) | 200 |
| `POST` | `/test/reset` | Delete all data | 204 |

---

## Core Business Logic

### Availability Check (half-open interval overlap)
Two bookings conflict if: `existingCheckIn < requestedCheckOut AND existingCheckOut > requestedCheckIn`

```csharp
// In RoomService.GetAvailableRoomsAsync
.Where(r => !r.Bookings.Any(b =>
    b.CheckInDate < checkOut &&
    b.CheckOutDate > checkIn))
```
Translates to a correlated `NOT EXISTS` subquery in SQL — no raw SQL needed.

### Email Normalisation (in BookingService before any DB operation)
```csharp
request.CustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant();
```
DTO validation: `[Required]`, `[EmailAddress]`, `[MinLength(1)]`
DB column: `nvarchar(200)` with `UseCollation("Latin1_General_CI_AS")`

### Booking Creation (transactional)
1. Normalise email (trim + lowercase)
2. Validate dates (checkOut > checkIn)
3. Begin `RepeatableRead` transaction
4. Fetch room, validate exists
5. Validate GuestCount <= room.Capacity
6. Re-check availability inside transaction
7. Generate unique `BK` + GUID reference
8. Insert booking, commit

### Error → HTTP mapping (GlobalExceptionHandler)
- `NotFoundException` → 404
- `ConflictException` → 409
- `BookingValidationException` → 400
- Unhandled → 500

---

## Seed Data

**Hotel 1: "The Grand Pelican"** — 2x Single (cap 1), 2x Double (cap 2), 2x Deluxe (cap 4)
**Hotel 2: "Harbour View Suites"** — same layout

Sample bookings:
- Double room at Hotel 1, June 1–5 2026, 2 guests
- Deluxe room at Hotel 2, June 10–12 2026, 3 guests

---

## NuGet Packages

**API project:**
- `Microsoft.EntityFrameworkCore.SqlServer` (10.x)
- `Microsoft.EntityFrameworkCore.Design` (10.x)
- `Swashbuckle.AspNetCore` (6.9.0)

**Test project:**
- `Microsoft.EntityFrameworkCore.InMemory`
- `xunit`, `xunit.runner.visualstudio`
- `Moq`

---

## docker-compose.yml

SQL Server 2022 container with health check, SA password from `.env`, persistent volume.
API auto-applies EF migrations on startup via `db.Database.MigrateAsync()`.

---

## Verification / Testing

1. `docker compose up -d` → SQL Server starts on port 1433
2. Run `HotelBooking.Api` in Rider → migrations auto-applied
3. Navigate to `/swagger` → Swagger UI
4. `POST /test/seed` → populate test data
5. `GET /hotels?name=Grand` → returns Hotel 1
6. `GET /hotels/1/rooms/available?checkIn=2026-06-01&checkOut=2026-06-05&guests=2` → booked Double room not listed
7. `POST /bookings` → create booking, receive `BK`-prefixed GUID reference
8. `GET /bookings/{reference}` → verify booking details
9. `POST /bookings` same room+dates → 409 Conflict
10. `POST /test/reset` → all data cleared
11. `dotnet test` → 17/17 unit tests pass

---

## Implementation Notes

- **.NET 10** used (Rider's bundled SDK — only `net10.0` available)
- **Swashbuckle 6.9.0** — downgraded from 10.x due to breaking API changes in `Microsoft.OpenApi` 3.x
- **In-memory DB** used for unit tests with `TransactionIgnoredWarning` suppressed
- **`dotnet ef` tool** requires dotnet on system PATH — see `run-migration.bat` approach if needed
