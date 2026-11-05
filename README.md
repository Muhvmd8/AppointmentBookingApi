# Appointment Booking System API

A RESTful Appointment Booking System built with **ASP.NET Core Web API**, designed as a technical assessment to automate appointment scheduling, employee management, and availability discovery.

The system addresses common scheduling challenges such as double booking, appointment conflicts, and limited visibility into employee schedules through a structured backend architecture with a focus on maintainability, data consistency, and performance.

## Business Overview

The application provides the following capabilities:

- **Appointment Booking:** Create appointments while preventing overlapping bookings for the same employee.
- **Availability Discovery:** Retrieve available time slots based on employee schedules and configurable booking rules.
- **Employee Scheduling:** Retrieve an employee's daily appointments with pagination.
- **Appointment Cancellation:** Cancel existing appointments and immediately release their time slots.
- **Employee Management:** Manage employees through CRUD-style operations, including activation and deactivation.

## Tech Stack

| Category | Technologies |
|---|---|
| Framework | ASP.NET Core Web API (.NET 9) |
| Language | C# |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Architecture | Onion Architecture |
| Mapping | AutoMapper |
| Logging | Serilog, ILogger |
| Log Monitoring | Serilog UI |
| Validation | FluentValidation |
| API Documentation | Swagger / OpenAPI |
| Localization | IStringLocalizer |
| Configuration | Options Pattern |
| Concurrency | Booking Lock Provider |
| Performance | Pagination, AsNoTracking, Indexing |
| Database Access | Repository & Unit of Work |

## Architecture & Design Decisions

### Onion Architecture

The application follows Onion Architecture to separate responsibilities and keep business logic independent of external infrastructure concerns.

The solution is organized into the following layers:

- **Domain:** Contains core business entities, rich domain models, business rules, domain errors, and enums.
- **Service:** Contains application business logic, service abstractions, DTO mapping, and orchestration of domain operations.
- **Infrastructure:** Handles database access, Entity Framework Core configurations, repositories, Unit of Work, and booking lock implementation.
- **API:** Exposes REST endpoints through controllers and handles HTTP concerns, middleware, and API response formatting.
- **Shared:** Contains shared DTOs, constants, enums, exceptions, and common response models.

### Repository & Unit of Work Patterns

The Repository Pattern abstracts database access and provides a consistent way to retrieve and manipulate entities.

The Unit of Work Pattern coordinates repository operations and database transactions, ensuring that related changes are committed or rolled back together.

### Rich Domain Models

Business rules are encapsulated inside domain entities rather than being scattered across services.

For example, the `Appointment` entity controls its own creation and cancellation through domain methods. This ensures that appointment state transitions are validated within the domain itself.

### Result Pattern

The Result Pattern is used to represent successful and failed business operations without relying on exceptions for expected business scenarios.

Services return `Result<T>` containing either the expected data or a domain/application error.

This provides:

- Explicit success and failure handling.
- Centralized error definitions.
- Separation between business errors and unexpected exceptions.
- Consistent error handling across services.

### Service Layer & IQueryable

Business operations are implemented in dedicated services, keeping controllers focused on HTTP request handling and response generation.

`IQueryable` is used within the Service Layer for database-side filtering, sorting, counting, and pagination.

Although exposing `IQueryable` through repository abstractions introduces some coupling to the underlying LINQ query provider (EF Core in this implementation), this was an intentional trade-off for a relatively small application with a fixed database provider.

It allows queries to be translated and executed directly by the database instead of loading unnecessary records into application memory.

The repository abstraction still keeps the infrastructure implementation outside the Domain layer.

### AutoMapper

AutoMapper is used to map domain entities to response DTOs, reducing repetitive mapping code and keeping API contracts separate from domain models.

## Key Features & Technical Implementations

### 1. Appointment Booking & Overlap Prevention

The booking workflow validates appointments before persisting them.

Implemented business rules include:

- Reject appointments scheduled in the past.
- Validate appointment duration (15–120 minutes).
- Ensure appointments fall within configured working hours.
- Verify that the assigned employee exists and is active.
- Prevent overlapping active appointments for the same employee.

The overlap condition is based on:

`Existing.StartAt < New.EndAt && Existing.EndAt > New.StartAt`

This handles partial overlaps, fully contained appointments, and appointments that contain existing bookings, while allowing adjacent appointments without conflicts.

### 2. Concurrency Control & Booking Locks

Preventing double booking requires more than checking whether an overlapping appointment already exists.

Two concurrent requests could both check availability before either request commits its booking.

To address this, a Booking Lock Provider is used to coordinate booking operations for the same employee and appointment date.

The booking workflow:

1. Begins a database transaction.
2. Acquires a lock for the employee and appointment date.
3. Checks employee availability and existing appointments.
4. Creates the appointment if no conflict exists.
5. Saves changes and commits the transaction.
6. Releases transactional resources through the lock provider's lifecycle.

The same locking strategy is applied during cancellation to prevent booking and cancellation operations from racing against each other.

### 3. Availability & Scheduling

Available slots are generated dynamically based on:

- Configured working hours.
- Employee's existing scheduled appointments.
- Requested appointment duration.
- Configurable slot interval.
- Current time.

The system retrieves existing appointments from the database and calculates available slots without generating unnecessary database queries for every individual time slot.

Employee schedules support pagination to avoid returning an unnecessarily large result set.

### 4. Appointment Cancellation

Appointments are cancelled through domain-level state transitions.

Cancelled appointments are excluded from overlap validation and availability calculations, immediately making their time slots available for future bookings.

### 5. Employee Management

The Employee Service provides:

- Create employee.
- Retrieve employee by ID.
- Retrieve employees with pagination.
- Update employee information.
- Deactivate employees.

Inactive employees cannot receive new appointments.

### 6. Performance Optimization

Several database and application-level optimizations are applied:

- **AsNoTracking:** Used for read-only queries to reduce Entity Framework Core change-tracking overhead.
- **Pagination:** Limits the number of records retrieved per request.
- **Database-side Filtering:** Uses IQueryable to execute filtering, sorting, and pagination at the database level.
- **Projection:** Retrieves only the required data when using projection-based queries.
- **Indexes:** Added to frequently queried columns to improve lookup and filtering performance.
- **Async Database Operations:** Uses asynchronous EF Core operations to avoid blocking request threads.
- **CancellationToken:** Propagated through asynchronous service and database operations to support request cancellation.

### 7. Unified API Responses

The API uses a consistent response structure through a centralized `ApiResponseFactory`.

Responses provide a unified contract containing:

- Success status.
- Response message.
- Returned data.
- Metadata.
- Pagination information when applicable.
- Trace ID for request correlation.

This makes API responses predictable and easier to consume and troubleshoot.

### 8. Centralized Exception Handling

A global exception-handling middleware handles unexpected application exceptions.

Expected business failures are represented through the Result Pattern and mapped to appropriate HTTP responses.

This avoids duplicating exception-handling logic across controllers and keeps HTTP error responses consistent.

### 9. Logging & Monitoring

The application uses **Serilog** for structured logging, alongside `ILogger<T>` for dependency-injected logging throughout the application.

Serilog UI is integrated to provide a centralized interface for inspecting application logs.

An authentication filter protects the logging dashboard from unauthorized access.

Logs include relevant contextual information such as employee IDs, appointment IDs, and booking timestamps to make troubleshooting easier.

### 10. Security & Request Protection

The API includes additional middleware and request protection mechanisms:

- **IP Security Middleware:** Provides IP-based request security checks.
- **Rate Limiting:** Restricts excessive incoming requests to reduce abuse and protect application resources.
- **Logging:** Records relevant security and operational events.

These measures provide an additional layer of protection around the API endpoints.

### 11. Configuration & Options Pattern

Application behavior is configured through strongly typed settings using the Options Pattern.

Booking-related configuration is encapsulated in `BookingSettings`, rather than scattering hardcoded values throughout the application.

This includes:

- Working start time.
- Working end time.
- Slot generation interval.
- Business time zone.

The configuration is injected into services through `IOptions<BookingSettings>`.

### 12. Localization

`IStringLocalizer` is used to support localized application messages and provide a centralized approach to handling user-facing text.

## Business Assumptions & Design Decisions

The assessment requirements leave some operational details open to interpretation. The following assumptions were made to establish consistent booking behavior.

### Working Hours

Working hours are assumed to be shared across all employees rather than individually configurable.

The default working schedule is:

- Start: 09:00 AM.
- End: 05:00 PM.

These values are configurable through `BookingSettings`.

### Time Zone Handling

All appointment timestamps are stored and compared in UTC to maintain consistency and avoid ambiguity when handling timestamps from different time zones.

However, business scheduling rules are evaluated using the configured business time zone.

The application uses `Africa/Cairo` as its default business time zone.

For example, if the working hours are configured as 09:00–17:00 in Cairo, the system converts those local boundaries to UTC before querying the database.

This means that working hours remain consistent with the company's local operating schedule, regardless of the UTC offset at a particular date.

### Slot Interval vs. Appointment Duration

The slot interval and appointment duration represent two different concepts.

- **Appointment Duration:** Determines how long an individual booking occupies an employee's schedule. The customer selects a duration between 15 and 120 minutes.
- **Slot Interval:** Determines how frequently the system generates potential appointment start times.

For example, with a 15-minute slot interval and a 60-minute appointment duration, the system can generate start times such as:

09:00, 09:15, 09:30, 09:45, 10:00.

Each generated slot occupies 60 minutes.

The interval is a scheduling granularity, not the duration of the appointment.

The slot interval is configurable and can be adjusted according to business requirements.

### Employee Scheduling

Employees are assumed to share the same standard working hours.

The system does not currently support individual employee shifts, breaks, holidays, or custom availability schedules.

### Appointment Status

Only appointments with the `Scheduled` status are considered active for overlap validation and availability calculations.

Cancelled appointments do not block time slots.

### Employee Lifecycle

Employees are deactivated instead of permanently deleted to preserve historical appointment references and maintain data consistency.

### Data Model

The initial database design consists of two primary entities:

- `Employee`
- `Appointment`

An employee can have multiple appointments, while each appointment is assigned to exactly one employee.

### Authentication & Authorization

Authentication and authorization were not implemented because they were explicitly excluded from the assessment requirements.

The logging dashboard is separately protected through an authentication filter.

### Employee Seeding

The database initialization and seeding infrastructure is prepared, but employee seed data has not been implemented.

Employees can be created through the Employee Management endpoints.

## Project Setup

### Prerequisites

- .NET 9 SDK.
- SQL Server.
- Visual Studio 2022 or a compatible IDE.
- EF Core CLI tools.

### 1. Clone the Repository

```bash
git clone <repository-url>
cd AppointmentBookingSystem
```

### 2. Configure the Connection String

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AppointmentBookingDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Adjust the connection string according to your SQL Server configuration.

### 3. Configure Booking Settings

Update the booking configuration:

```json
{
  "BookingSettings": {
    "WorkingStart": "09:00",
    "WorkingEnd": "17:00",
    "SlotIntervalMinutes": 15,
    "TimeZone": "Africa/Cairo"
  }
}
```

### 4. Apply Database Migrations

```bash
dotnet ef database update
```

Make sure to execute the command against the project containing the EF Core migrations and configure the appropriate startup project if required.

### 5. Run the Application

```bash
dotnet run
```

### 6. Explore the API

Once the application is running, navigate to the Swagger endpoint to explore and test the available API operations.

## API Documentation

The API exposes endpoints for:

| Resource | Operations |
|---|---|
| Employees | Create, Get By ID, Get All, Update, Deactivate |
| Appointments | Create, Cancel |
| Availability | Get Available Slots |
| Employee Schedule | Get Daily Schedule |

Swagger/OpenAPI provides interactive API documentation and request testing.

## Project Scope & Future Improvements

The current implementation focuses on the assessment requirements and the core appointment scheduling lifecycle.

Potential future improvements include:

- Individual employee working schedules.
- Employee breaks, holidays, and time-off management.
- Automated employee seed data.
- Integration and concurrency testing.
- Distributed booking locks for multi-instance deployments.
- Authentication and role-based authorization.
- Automated notifications for appointment creation and cancellation.

## Final Notes

This project was developed as a practical technical assessment following a technical interview, with the goal of demonstrating backend engineering fundamentals, architectural separation, business rule enforcement, database consistency, and practical performance considerations.

The implementation prioritizes clear business logic, maintainable code, and reliable appointment scheduling rather than unnecessary architectural complexity.
